using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PROG6212PoE_ST10440987.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        [Required]
        [Range(0.01, 2000, ErrorMessage = "Hours Worked must be a positive number and not be more than 160 hours")]
        public int hoursWorked { get; set; }
        public double hourlyRate { get; set; }

        [Required(ErrorMessage = "Additional Notes is required")]
        public string additionalNotes { get; set; }
        [BindNever]
        public string? claimStatus { get; set; }
        [BindNever]
        public DateTime? dateOfClaim { get; set; }
        public bool potentialBadClaim {  get; set; } = false;
        public string? badClaimMessage { get; set; }

        [ValidateNever]
        public User? Lecturer { get; set; }
        [BindNever]
        public string? LecturerId { get; set; }

        [NotMapped]
        public IFormFile? UploadedSupportingDocument { get; set; }
        public SupportingDocument? SupportingDocument { get; set; }
    }
}
