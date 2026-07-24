using System.ComponentModel.DataAnnotations;

namespace AurumFinance.Models
{
    public class ChartOfAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Reference number is required.")]
        public int ReferenceNumber { get; set; }

        [Required(ErrorMessage = "Account name is required.")]
        [StringLength(100)]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Account type is required.")]
        public string Type { get; set; }

        [Required(ErrorMessage = "System role is required.")]
        public string Role { get; set; }

        public decimal Balance { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}