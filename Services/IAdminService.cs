using Microsoft.AspNetCore.Identity;
using Real_State.Controllers;
using Real_State.Models;
using Real_State.Tables;

namespace Real_State.Services
{
    public interface IAdminService
    {
        Task<bool> UpdateUserProfileAsync(HttpContext httpContext,string userId,ProfileModel model);
        Task<(bool success, string message)> TogglePropertySaveStatusAsync(string userId, int propertyId);
        Task<List<AgentModel>> GetAgentsRecursive(List<Property> properties, int index);
        Task<List<PropertiesSavedStatus>> GetPropertiesSavedStatusRecursive(
            List<Property> properties, string userId, int index);
        public string SaveLocationsToFile(List<string> locations, string filePath);
        List<string> ReadLocationsFromFile(string filePath);
        bool IsLocationUsedByAnyUser(string location);
    }
}
