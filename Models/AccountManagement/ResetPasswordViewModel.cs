using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NetMudCore.Models.AccountManagement
{
    public class ResetPasswordViewModel
    {
        [SetsRequiredMembers]
        public ResetPasswordViewModel(string email, string password, string confirmPassword, string code) =>
            (Email, Password, ConfirmPassword, Code) = (email, password, confirmPassword, code);

        [Required]
        [EmailAddress]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
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

        [DataType(DataType.Text)]
        public required string Code { get; set; }
    }
}
