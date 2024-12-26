using System.ComponentModel.DataAnnotations;

namespace Real_State.Models
{
    public class AddPropertyModel
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Type { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int Space { get; set; }
        public int Price { get; set; }
        public List<string> GizaLocations { get; set; }
        public List<string> CairoLocations { get; set; }
        public IFormFile MainImage { get; set; }
        public List<IFormFile> AdditionalImages { get; set; }

    }

    public class UpdatePropertyModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Type { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int Space { get; set; }
        public int Price { get; set; }
        public List<string> GizaLocations { get; set; }
        public List<string> CairoLocations { get; set; }

        public IFormFile MainImage { get; set; }
        public string MainImageURL { get; set; }
        public List<IFormFile> AdditionalImages { get; set; }
        public List<string> AdditionalImagesURL { get; set; }

    }
}
