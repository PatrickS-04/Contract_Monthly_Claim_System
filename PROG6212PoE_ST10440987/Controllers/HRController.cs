using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212PoE_ST10440987.Data;
using PROG6212PoE_ST10440987.Models;

namespace PROG6212PoE_ST10440987.Controllers
{
    public class HRController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _appDbContext;

        public HRController(UserManager<User> userManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
        }

        // ---------------------------------------------------------------------------------------------------
        // This method returns the Index view
        public IActionResult Index()
        {
            return View();
        }

        // ---------------------------------------------------------------------------------------------------
        // This method shows the add new user form to user
        public IActionResult AddNewUser()
        {
            return View();
        }

        // ---------------------------------------------------------------------------------------------------
        // This method adds the new users details input via the form to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewUser(CreateUserViewModel newUserModel)
        {
            if (!ModelState.IsValid)
            {
                return View(newUserModel);
            }

            var user = new User
            {
                UserName = newUserModel.UserName,
                Email = newUserModel.Email,
                Name = newUserModel.Name,
                Surname = newUserModel.Surname,
                HourlyRate = newUserModel.HourlyRate
            };

            var createUser = await _userManager.CreateAsync(user, newUserModel.Password);

            if (createUser.Succeeded)
            {
                var roleOfUser = await _userManager.AddToRoleAsync(user, newUserModel.SelectedRole);

                if (roleOfUser.Succeeded)
                {
                    TempData["SuccessMessage"] = $"User {user.UserName} was created successfully with role: {newUserModel.SelectedRole}";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in roleOfUser.Errors)
                    {
                        ModelState.AddModelError(string.Empty, $"Could not assign role to user because: {error.Description}");
                    }
                    return View(newUserModel);
                }
            }
            else
            {
                foreach (var error in createUser.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(newUserModel);
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows HR to view all the existing users
        public async Task<IActionResult> ShowAllUsers()
        {
            var existingUsers = await _appDbContext.Users.ToListAsync();

            if (existingUsers == null)
            {
                return View(new List<User>());
            }

            return View(existingUsers);
        }

        // ---------------------------------------------------------------------------------------------------
        // This method gets the details of the user thats details are being edited
        [HttpGet]
        public async Task<IActionResult> EditUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["NoUserFoundErrorMessage"] = "User with that Id cannot be found";
                return RedirectToAction("ShowAllUsers");
            }

            var model = new CreateUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                UserName = user.UserName,
                HourlyRate = user.HourlyRate,
                SelectedRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault(),
                Password = "********"
            };

            return View(model);
        }

        // ---------------------------------------------------------------------------------------------------
        // This is the post method to update the details of the users upon submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserDetails(CreateUserViewModel editUserDetailsModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editUserDetailsModel);
            }

            var updateUser = await _userManager.FindByIdAsync(editUserDetailsModel.Id);

            if (updateUser == null)
            {
                TempData["NoUserFoundErrorMessage"] = "User with that Id cannot be found";
                return RedirectToAction("ShowAllUsers");
            }

            updateUser.Name = editUserDetailsModel.Name;
            updateUser.Surname = editUserDetailsModel.Surname;
            updateUser.Email = editUserDetailsModel.Email;
            updateUser.UserName = editUserDetailsModel.UserName;
            updateUser.HourlyRate = editUserDetailsModel.HourlyRate;

            var updateDetails = await _userManager.UpdateAsync(updateUser);

            if (updateDetails.Succeeded)
            {
                var role = await _userManager.GetRolesAsync(updateUser);
                var selectedRole = role.FirstOrDefault();

                if (selectedRole != editUserDetailsModel.SelectedRole)
                {
                    var removeRole = await _userManager.RemoveFromRolesAsync(updateUser, role);

                    if (removeRole.Succeeded)
                    {
                        var addRole = await _userManager.AddToRoleAsync(updateUser, editUserDetailsModel.SelectedRole);
                    }
                }

                TempData["SuccessMessage"] = "Users details were successfully updated";
                return RedirectToAction("ShowAllUsers");
            }
            else
            {
                foreach (var error in updateDetails.Errors)
                {
                    ModelState.AddModelError(string.Empty, $"Update of User details failed: {error.Description}");
                }
            }
            return View(editUserDetailsModel);
        }

        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GenerateSummaryReport()
        {
            var approvedClaims = await _appDbContext.Claims.Include(claim => claim.Lecturer)
                .Where(claim => claim.claimStatus == "Approved").ToListAsync();

            var summaryForLecturer = approvedClaims.GroupBy(claim => claim.Lecturer)
                .Select(group =>
                {
                    var totalHoursWorked = group.Sum(claim => claim.hoursWorked);
                    var hourlyRate = group.Key.HourlyRate;

                    return new LecturerReportSummary
                    {
                        LecturerId = group.Key.Id,
                        LecturerName = $"{group.Key.Name} {group.Key.Surname}",
                        LecturerEmail = group.Key.Email,
                        HourlyRate = hourlyRate,
                        OverallHoursWorked = totalHoursWorked,
                        OverallAmountDue = totalHoursWorked * hourlyRate,

                        ApprovedClaims = group.Select(claim => new ApprovedClaim
                        {
                            ClaimId = claim.ClaimId,
                            dateOfClaim = claim.dateOfClaim ?? DateTime.MinValue,
                            HoursWorked = claim.hoursWorked,
                            totalForClaim = claim.hoursWorked * hourlyRate
                        }).ToList()
                    };
                }).ToList();

            var report = new ReportSummaryViewModel
            {
                lecturerReportSummaries = summaryForLecturer,
                TotalHoursWorked = (int)Math.Round(summaryForLecturer.Sum(summary => summary.OverallHoursWorked)),
                TotalAmountDue = summaryForLecturer.Sum(summary => summary.OverallAmountDue),
                ReportDate = DateTime.Now.ToString("MMMM")
            };

            return View(report);
        }
    }
    // -------------------------------------------------------000... End of File ...000--------------------------------------------------------------------
}
