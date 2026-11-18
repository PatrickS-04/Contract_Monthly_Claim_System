using Microsoft.AspNetCore.Identity;

namespace PROG6212PoE_ST10440987.Models
{
    public class User : IdentityUser
    {
        public string Name {  get; set; }
        public string Surname { get; set; }
        public double HourlyRate { get; set; }
        public bool ChangePassword { get; set; } = true;
    }
}
