namespace Real_State.Models
{
    public class LocationViewModel
    {
        public List<LocationItem> CairoLocations { get; set; }
        public List<LocationItem> GizaLocations { get; set; }
    }

    public class LocationItem
    {
        public string Name { get; set; }
        public bool IsUsed { get; set; }
    }

    public class LocationRequestModel
    {
        public List<string> CairoLocations { get; set; }
        public List<string> GizaLocations { get; set; }
    }
}
