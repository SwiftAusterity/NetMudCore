using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NetMudCore.Models.AccountManagement
{
    public class ForgotPasswordViewModel
    {
        [SetsRequiredMembers]
        public ForgotPasswordViewModel(string email) => (Email) = (email);

        [Required]
        [EmailAddress]
        [Display(Name = "Email", Description = "The email address used to register your account. Also your username for logging in.")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }
    }
}
