namespace Real_State.Tables
{
    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public int Space { get; set; }
        public string Purpose { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int Sapce { get; set; }
        public int Price { get; set; }
        public string? MainImagePath { get; set; }
        public ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();

        public ICollection<UserProperty> UserProperties { get; set; }

        public bool IsSold { get; set; }
        public string? SoldDescription { get; set; }
        public ICollection<Appointment> Appointments { get; set; }


    }
}
