namespace Real_State.Tables
{
    public class Wallet
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<User> Admins { get; set; } // Admins using this wallet (for many-to-one relationship)

        public ICollection<Transaction> Transactions { get; set; }
    }
}
