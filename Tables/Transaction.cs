using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Real_State.Tables
{
    public class Transaction
    {
        public int Id { get; set; } 

        [Required]
        public int WalletId { get; set; } 

        public Wallet Wallet { get; set; } 

        [Required]
        public string Type { get; set; } 

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } 

        [StringLength(255)]
        public string Description { get; set; }

        public int? PropertyId { get; set; }

        public Property? Property { get; set; }
    }
}
