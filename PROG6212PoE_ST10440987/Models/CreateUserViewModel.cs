using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PROG6212PoE_ST10440987.Models
{
    public class CreateUserViewModel
    {
        [ValidateNever]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [Range(0.01, 2000, ErrorMessage = "Hourly Rate must be a positive number")]
        public double HourlyRate { get; set; }
        [Required]
        [Display(Name = "User Role")]
        public string SelectedRole { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
