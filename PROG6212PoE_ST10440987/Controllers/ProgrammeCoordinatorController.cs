using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212PoE_ST10440987.Data;
using PROG6212PoE_ST10440987.Models;

namespace PROG6212PoE_ST10440987.Controllers
{
    public class ProgrammeCoordinatorController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public ProgrammeCoordinatorController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // ---------------------------------------------------------------------------------------------------
        // This method returns the index view
        public IActionResult Index()
        {
            return View();
        }

        // ---------------------------------------------------------------------------------------------------
        /* This method allows Academic Managers and Programme Coordinators to view Pending Claims submitted
           by Lecturers */
        public IActionResult ViewPendingClaims()
        {
            // looks to see if the lecturers username exists in the database
            var lecturer = _appDbContext.Users.FirstOrDefault(lecturer => lecturer.UserName == User.Identity.Name);

            // If user does not exist then an empty list is returned
            if (lecturer == null)
            {
                return View(new List<Claim>());
            }

            // Only shows claims with claimStatus of "Pending" and returns the list of claims
            var pendingClaims = _appDbContext.Claims.Where(claim => claim.claimStatus == "pending")
                .Include(claim => claim.SupportingDocument)
                .Include(claim => claim.Lecturer).ToList();
            return View(pendingClaims);
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows Academic Managers and Programme Coordinators to Approve claims
        [HttpPost]
        public async Task<IActionResult> VerifyClaim(int claimId)
        {
            // try-catch that gets the claim by its ClaimId and if claim is null then shows error message
            try
            {
                var claim = await _appDbContext.Claims
                    .FirstOrDefaultAsync(claim => claim.ClaimId == claimId);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "No Claim was found!";
                    return RedirectToAction("ViewPendingClaims");
                }

                // if claim is found, the claimStatus is changed to "Approved" and saved to database
                claim.claimStatus = "Verified";
                await _appDbContext.SaveChangesAsync();
            }
            // catch to show an error if something goes wrong with approving the claim
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unfortunately something went wrong with verifying the claim. Please retry!";
            }
            return RedirectToAction("ViewPendingClaims");
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows Academic Managers and Programme Coordinators to Reject claims 
        [HttpPost]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            // try-catch that gets claim by ClaimId and shows error if no claim is found
            try
            {
                var claim = await _appDbContext.Claims
                    .FirstOrDefaultAsync(claim => claim.ClaimId == claimId);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "No Claim was found!";
                    return RedirectToAction("ViewPendingClaims");
                }

                // if claim was found, claimStatus is changed to "Rejected" and saved to database
                claim.claimStatus = "Rejected";
                await _appDbContext.SaveChangesAsync();
            }
            // catch to show an error if something goes wrong with rejecting the claim
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unfortunately something went wrong with rejecting the claim. Please retry!";
            }
            return RedirectToAction("ViewPendingClaims");
        }

        // ---------------------------------------------------------------------------------------------------
        /* This method allows Academic Managers and Programme Coordinators to download the supporting
           document if one was provided */
        public async Task<IActionResult> DownloadSupportingDocument(int claimId)
        {
            // gets the document if there is one via the ClaimId
            var claim = await _appDbContext.Claims.Include(claim => claim.SupportingDocument)
                .FirstOrDefaultAsync(claim => claim.ClaimId == claimId);

            // if claim is not found, then an error shows saying no document
            if (claim == null)
            {
                return NotFound("No document was found for this claim");
            }

            // returns the file to download
            return File(claim.SupportingDocument.dataFromFile, "application/octet-stream",
                $"Claim_Document_{claim.ClaimId}{claim.SupportingDocument.fileType}");
        }
    }
    // -------------------------------------------------------000... End of File ...000--------------------------------------------------------------------
}
