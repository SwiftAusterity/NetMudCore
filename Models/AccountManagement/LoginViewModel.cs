using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NetMudCore.Models.AccountManagement
{
    public class LoginViewModel
    {
        [SetsRequiredMembers]
        public LoginViewModel(string email, string password) =>
           (Email, Password) = (email, password);

        [Required]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [UIHint("Password")]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [UIHint("Boolean")]
        [Display(Name = "Remember me?", Description = "Retain this login state in a cookie for future use.")]
        public bool RememberMe { get; set; }
    }
}
