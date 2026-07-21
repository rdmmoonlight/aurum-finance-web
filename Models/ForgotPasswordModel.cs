using System.ComponentModel.DataAnnotations;

namespace AurumFinance.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;
    }
}
