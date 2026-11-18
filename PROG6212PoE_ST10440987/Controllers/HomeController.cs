using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROG6212PoE_ST10440987.Models;

namespace PROG6212PoE_ST10440987.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ---------------------------------------------------------------------------------------------------
        // This method returns the index view
        public IActionResult Index()
        {
            return View();
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows users to login by entering their username and password
        [HttpPost]
        public async Task<IActionResult> Login()
        {
            string userName = Request.Form["username"];
            string password = Request.Form["password"];

            var signIn = await _signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: false);

            // if login is successful the statement executes
            if (signIn.Succeeded)
            {
                // find the user and check what their role is
                var user = await _userManager.FindByNameAsync(userName);

                // If user is found and the changePassword variable is true, then direct user to ChangePassword view
                if (user != null && user.ChangePassword)
                {
                    return RedirectToAction("ChangePassword", "Home");
                }

                var userRole = await _userManager.GetRolesAsync(user);

                // if role is Lecturer, navigate to lecturer index
                if (userRole.Contains("Lecturer"))
                {
                    return RedirectToAction("Index", "Lecturer");
                }

                // if role is Academic Manager, navigate to Academic Manager index
                if (userRole.Contains("Academic Manager"))
                {
                    return RedirectToAction("Index", "AcademicManager");
                }

                // if role is Programme Coordinator, navigate to Programme Coordinator index
                if (userRole.Contains("Programme Coordinator"))
                {
                    return RedirectToAction("Index", "ProgrammeCoordinator");
                }

                // if role is HR, navigate to HR index
                if (userRole.Contains("HR"))
                {
                    return RedirectToAction("Index", "HR");
                }

                return RedirectToAction("Index");
            }

            // if login was unsuccessful, show error and user must retry
            ModelState.AddModelError(string.Empty, "Incorrect Username or Password. Please try again");
            return View("Index");
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows users to logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------------------------------------------------------------------------------------------------
        // This method displays the change password view
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            return View(model);
        }

        // ---------------------------------------------------------------------------------------------------
        // This method forces new users to change their temporary password to a permanent custom password
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel passwordModel)
        {
            if (!ModelState.IsValid)
            {
                return View(passwordModel);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var passwordChanged = await _userManager.ChangePasswordAsync(user, passwordModel.OldPassword, passwordModel.NewPassword);

            if (passwordChanged.Succeeded)
            {
                user.ChangePassword = false;

                var update = await _userManager.UpdateAsync(user);

                if (update.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = "Password was successfully set. Welcome!";
                    var userRole = await _userManager.GetRolesAsync(user);

                    if (userRole.Contains("Lecturer"))
                    {
                        return RedirectToAction("Index", "Lecturer");
                    }
                    else if (userRole.Contains("Programme Coordinator"))
                    {
                        return RedirectToAction("Index", "ProgrammeCoordinator");
                    }
                    else if (userRole.Contains("Academic Manager"))
                    {
                        return RedirectToAction("Index", "AcademicManager");
                    }
                    else if (userRole.Contains("HR"))
                    {
                        return RedirectToAction("Index", "HR");
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Password was set successfully but couldn't update login status");
                }
            }
            else
            {
                foreach (var error in passwordChanged.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(passwordModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    // -------------------------------------------------------000... End of File ...000--------------------------------------------------------------------
}
