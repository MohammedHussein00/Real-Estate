using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Real_State.Data;
using Real_State.Models;
using Real_State.Tables;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;

namespace Real_State.Services
{
    public class UserService: IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(UserManager<IdentityUser> userManager,
    IHttpContextAccessor httpContextAccessor
    , RoleManager<IdentityRole> roleManager
    , ApplicationDbContext Context)
        {
            _Context = Context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
        }


        public async Task<bool> AddNotificationAsync(string content, string userId)
        {
            try
            {
                var notification = new Notification
                {
                    Content = content,
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                _Context.Notifications.Add(notification);
                await _Context.SaveChangesAsync();

                return true; // Return true if save was successful
            }
            catch
            {
                return false; // Ignore exceptions and return false
            }
        }



       

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || !await _userManager.CheckPasswordAsync(user, oldPassword))
            return false;
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }
        public async Task<bool> SendEmail(string userName, string userEmail, string subject, string textContent)
        {
            sib_api_v3_sdk.Client.Configuration.Default.ApiKey["api-key"] = "";

            // Create an instance of the TransactionalEmailsApi
            var apiInstance = new TransactionalEmailsApi();

            // Define sender details
            string senderName = "Real Estate";
            string senderEmail = "mh2156@fayoum.edu.eg";
            SendSmtpEmailSender sender = new SendSmtpEmailSender(senderName, senderEmail);

            // Define recipient details
            string toEmail = userEmail;
            string toName = userName;
            SendSmtpEmailTo recipient = new SendSmtpEmailTo(toEmail, toName);
            List<SendSmtpEmailTo> recipients = new List<SendSmtpEmailTo>();
            recipients.Add(recipient);

            try
            {
                var sendSmtpEmail = new SendSmtpEmail(sender, recipients, null, null, textContent, "Verification Email", subject); // Pass an empty list for Bcc

                // Send the email
                CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                return true;


            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
