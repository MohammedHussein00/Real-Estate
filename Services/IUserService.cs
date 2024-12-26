using System.Threading.Tasks;

namespace Real_State.Services
{
    public interface IUserService
    {
        Task<bool> AddNotificationAsync(string content, string userId);
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<bool> SendEmail(string userName, string userEmail, string subject, string textContent);

    }
}
