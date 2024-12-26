namespace Real_State.Tables
{
    public class Notification
    {
        public int Id { get; set; }
        public string Content { get; set; }

        // Additional properties
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        // Optional foreign key for user
        public string UserId { get; set; }
        public User? User { get; set; }  // Assuming a User class exists
    }
}
