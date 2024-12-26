namespace Real_State.Models
{
    public class ProfileModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Location { get; set; }
        public IFormFile? Photo { get; set; }
        public List<string> CairoLocations { get; set; }
        public List<string> GizaLocations { get; set; }
    }
}
