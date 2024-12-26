using Real_State.Tables;

namespace Real_State.Models
{
    public class PropertyDetailsModel
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

        public bool IsSaved { get; set; }
  
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentEmail { get; set; }
        public string AgentPhone { get; set; }
        public string AgentImagePath { get; set; }
        public List<string> AdditionalImages { get; set; }
        public bool IsSold { get; set; }
        public int TotalViews { get; set; }


    }
}
