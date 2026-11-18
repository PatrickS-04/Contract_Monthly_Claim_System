using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PROG6212PoE_ST10440987.Data;
using PROG6212PoE_ST10440987.Models;
using Aes = System.Security.Cryptography.Aes;

namespace PROG6212PoE_ST10440987.Controllers
{
    public class LecturerController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;

        public LecturerController(SignInManager<User> signInManager, AppDbContext appDbContext, IConfiguration config, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _appDbContext = appDbContext;
            _config = config;
            _userManager = userManager;
        }

        // ---------------------------------------------------------------------------------------------------
        // This method returns the index view
        public IActionResult Index()
        {
            return View();
        }

        // ---------------------------------------------------------------------------------------------------
        // This method returns the SubmitClaim view
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SubmitClaim()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Index", "Lecturer");
            }

            var claim = new Claim
            {
                hourlyRate = user.HourlyRate,
            };
            return View(claim);
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows Lecturers to Submit a Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(500000000)]
        public async Task<IActionResult> SubmitClaim(Claim claimModel)
        {
            if (!ModelState.IsValid)
            {
                return View(claimModel);
            }

            const long MAX_FILE_SIZE = 35 * 1024 * 1024;

            // if there is a document that was uploaded, execute
            if (claimModel.UploadedSupportingDocument != null && claimModel.UploadedSupportingDocument.Length > 0)
            {
                // if the file size is larger than MAX_FILE_SIZE, show error saying its too large
                if (claimModel.UploadedSupportingDocument.Length > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("UploadedSupportingDocument", "File is too large. The file has to be less than 35 MB");
                    return View(claimModel);
                }

                // copy the data from the file and encrypt it
                using (var memoryStream = new MemoryStream())
                {
                    await claimModel.UploadedSupportingDocument.CopyToAsync(memoryStream);

                    byte[] fileBytes = memoryStream.ToArray();
                    byte[] encryptedFileData = EncryptUploadedFile(fileBytes);

                    claimModel.SupportingDocument = new SupportingDocument
                    {
                        fileType = Path.GetExtension(claimModel.UploadedSupportingDocument.FileName),
                        maxFileLimit = MAX_FILE_SIZE,
                        dataFromFile = encryptedFileData,
                        Claim = claimModel
                    };
                }
            }

            // find the lecturer to link the claim to
            var lecturer = _appDbContext.Users.FirstOrDefault(user => user.UserName == User.Identity.Name);

            claimModel.dateOfClaim = DateTime.Now;
            claimModel.claimStatus = "Pending";
            claimModel.hourlyRate = lecturer.HourlyRate;

            // show error if no lecturer found
            if (lecturer == null)
            {
                ModelState.AddModelError("", "Cannot find User!");
                return View(claimModel);
            }

            claimModel.LecturerId = lecturer.Id;

            if (claimModel.hoursWorked > 160)
            {
                ModelState.AddModelError(nameof(claimModel.hoursWorked), "Hours worked exceeds the monthly allowed hours worked");
                claimModel.potentialBadClaim = true;
                claimModel.badClaimMessage += "Hours worked exceeds the monthly allowed hours worked";
            }

            if (string.IsNullOrWhiteSpace(claimModel.additionalNotes))
            {
                ModelState.AddModelError(nameof(claimModel.additionalNotes), "Additional Notes are required");
                claimModel.potentialBadClaim = true;
                claimModel.badClaimMessage += "Additional Notes are required";
            }

            // save claim to database and return to Lecturer index view
            _appDbContext.Claims.Add(claimModel);
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Lecturer");
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows Lecturers to view the claims that they have submitted for verification
        public async Task<IActionResult> ViewLecturerClaims()
        {
            var lecturer = await _appDbContext.Users.FirstOrDefaultAsync(lecturer => lecturer.UserName == User.Identity.Name);

            if (lecturer == null)
            {
                return View(new List<Claim>());
            }

            // show claims as well as the supporting document if there is one
            var claims = await _appDbContext.Claims.Where(claim => claim.LecturerId == lecturer.Id)
                .Include(claim => claim.SupportingDocument).ToListAsync();
            return View(claims);
        }

        // ---------------------------------------------------------------------------------------------------
        // This method allows Lecturers to download the supporting documents if there is one
        public async Task<IActionResult> DownloadSupportingDocument(int claimId)
        {
            // find claim by ClaimId and allow for download of the supporting document if there is one and decrypt it
            var claim = await _appDbContext.Claims.Include(claim => claim.SupportingDocument)
                .FirstOrDefaultAsync(claim => claim.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound("No document was found for this claim");
            }

            var encryptedBytes = claim.SupportingDocument.dataFromFile;
            
            var decryptedFile = DecryptSupportingDocument(encryptedBytes);

            return File(decryptedFile, "application/octet-stream", 
                $"Claim_Document_{claim.ClaimId}{claim.SupportingDocument.fileType}");
        }

        // ---------------------------------------------------------------------------------------------------
        // This method Encrypts the uploaded file
        private byte[] EncryptUploadedFile(byte[] fileBytes)
        {
            var key = _config["Encryption:key"];
            var iv = _config["Encryption:iv"];

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            {
                throw new Exception("No key or iv found in the configuration");
            }

            var keyFromBase64 = Convert.FromBase64String(key);
            var ivFromBase64 = Convert.FromBase64String(iv);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = keyFromBase64;
                aes.IV = ivFromBase64;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(fileBytes, 0, fileBytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // This method decrypts the file before user downloads it
        private byte[] DecryptSupportingDocument(byte[] encryptedBytes)
        {
            var key = Convert.FromBase64String(_config["Encryption:key"]);
            var iv = Convert.FromBase64String(_config["Encryption:iv"]);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }
                    return memoryStream.ToArray();
                }
            }
        }
    }
    // -------------------------------------------------------000... End of File ...000--------------------------------------------------------------------
}
