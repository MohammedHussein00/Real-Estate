namespace Real_State.Models
{
    public class AuthModel
    {


        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImgUrl { get; set; }

        public List<string> Roles { get; set; }
        //not used
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }
        public bool EmailConfirmed { get; set; }

    }
}
