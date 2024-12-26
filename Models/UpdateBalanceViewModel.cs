using System.ComponentModel.DataAnnotations;

namespace Real_State.Models
{
    public class UpdateBalanceViewModel
    {
        [Required]
        public string Type { get; set; } // Deposit or Withdrawal

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Description can't exceed 100 characters.")]
        public string Description { get; set; }
    }
}
