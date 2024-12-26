using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Real_State.Tables;
using Real_State.Models;

namespace Music_Aditor.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterUserAsync(RegisterModel model);
        Task<AuthModel> RegisterAdminAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(LoginModel model);
        User CreateUser();
    }

}
