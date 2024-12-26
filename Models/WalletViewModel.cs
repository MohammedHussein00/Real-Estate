namespace Real_State.Models
{
    public class WalletViewModel
    {
        public decimal Balance { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
    }
}
