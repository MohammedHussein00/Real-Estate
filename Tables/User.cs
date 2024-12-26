using Microsoft.AspNetCore.Identity;

namespace Real_State.Tables
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }
        public string? image { get; set; }
        public ICollection<UserProperty> UserProperties { get; set; }
        public ICollection<Notification> Notifications { get; set; }

        public int WalletId { get; set; }
        public Wallet? Wallet { get; set; }
        public string? Location { get; set; }



    }
}
