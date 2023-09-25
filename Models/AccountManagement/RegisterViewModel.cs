using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NetMudCore.Models.AccountManagement
{
    public class RegisterViewModel
    {
        [SetsRequiredMembers]
        public RegisterViewModel(string email, string password, string confirmPassword, string globalUserHandle) => 
            (Email, Password, ConfirmPassword, GlobalUserHandle) = (email, password, confirmPassword, globalUserHandle);

        [Required]
        [EmailAddress]
        [Display(Name = "Email", Description = "Your email address is only stored for the purposes of sending Reset and Forgot Password emails. Also your username for logging in.")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Global User Handle", Description = "The name you will be identified by throughout the system. Not your game character's name.")]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Text)]
        public required string GlobalUserHandle { get; set; }

        public bool NewUserLocked { get; set; }
    }
}
