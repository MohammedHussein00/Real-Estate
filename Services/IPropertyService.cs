using Microsoft.AspNetCore.Identity;
using Real_State.Tables;

namespace Real_State.Models
{
    public interface IPropertyService
    {
        Task<List<Property>> GetSavedProperties(string userId);
        Task<bool> RemoveSavedProperty(string userId, int propertyId);
        Task<bool> RemoveAllSavedProperties(string userId);
        Task<int> AddProperty(AddPropertyModel model);
        Task SendNotificationToUser(IdentityUser user, string description);
        Task<List<Tables.Property>> GetAllProperties(List<Tables.Property> allProperties, string location = null, string purpose = null, double minPrice = 40000, double maxPrice = 1000000,
   double minSpace = 40, double maxSpace = 3000, int bedrooms = 0, int bathrooms = 0, string type = "Select Property Type", string sort = "Sort by");
        Task<bool> RemoveProperty(int propertyId);
        Task<bool> UpdateProperty(int propertyId, UpdatePropertyModel model);
    }

}
