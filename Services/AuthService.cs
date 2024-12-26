
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Real_State.Data;
using Real_State.Models;
using Real_State.Tables;
using Real_State.Services;

namespace Music_Aditor.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly IPropertyService _IpropertyService;

        private readonly IUserService _userService;

        public AuthService(UserManager<IdentityUser> userManager
           
            , RoleManager<IdentityRole> roleManager
            , ApplicationDbContext Context,
            IPropertyService IpropertyService,
            IUserService userService
            )
        {
            _Context = Context;
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _IpropertyService = IpropertyService;
        }


        public User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }


        public async Task<AuthModel> RegisterUserAsync(RegisterModel model)
        {
            // Check if the email is already registered
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered" };

            if (await _Context.Users.FirstOrDefaultAsync(s => s.PhoneNumber == model.Phone) is not null)
                return new AuthModel { Message = "Phone is already registered" };

            // If using email as username, this check may be redundant
            if (await _userManager.FindByNameAsync(model.Email) is not null)
                return new AuthModel { Message = "Username already exists" };

            // Create a new user
            var user = CreateUser();
            user.UserName = model.Email; // Assign email as username
            user.Email = model.Email;
            user.Name = model.Name;
            user.PhoneNumber = model.Phone;

            // Create a wallet and save it first
            var wallet = new Wallet
            {
                Balance = 0, // Initial balance
                CreatedDate = DateTime.UtcNow,
            };

            await _Context.Wallets.AddAsync(wallet); // Add wallet to database
            await _Context.SaveChangesAsync(); // Save the wallet to get its ID

            // Assign the WalletId to the user
            user.WalletId = wallet.Id;

            // Create the user with the specified password
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            // Assign the "User" role to the newly created user
            await _userManager.AddToRoleAsync(user, "User");
            await _IpropertyService.SendNotificationToUser(user, "Welcome to our Real Estate Website");
            await _userService.SendEmail("test", user.UserName, "verify your email", "Welcome to our Real Estate Website<br/><br/><a href='https://localhost:7199/'>Visit our site</a>");

            await _IpropertyService.SendNotificationToUser(user, "Update location in your profile to get notification of properties that suitable with your location");
            await _userService.SendEmail("test", user.UserName, "verify your email", $"Update location in your profile to get notification of properties that suitable with your location<br/><br/><a href='https://localhost:7199/User/Profile'>Go to update your profile</a>");

            // Return the authentication model
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = DateTime.Now.AddDays(30),
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = "", // Add token generation logic if required
                Name = user.Name,
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<AuthModel> RegisterAdminAsync(RegisterModel model)
        {
            // Check if the email is already registered
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered" };

            if (await _Context.Users.FirstOrDefaultAsync(s => s.PhoneNumber == model.Phone) is not null)
                return new AuthModel { Message = "Phone is already registered" };

            // If using email as username, this check may be redundant, consider removing
            if (await _userManager.FindByNameAsync(model.Email) is not null)
                return new AuthModel { Message = "Username already exists" };

            // Create a new student user
            var user = CreateUser();
            user.UserName = model.Email; // Assigning email as the username
            user.Email = model.Email;
            user.Name = model.Name;
            user.PhoneNumber = model.Phone;
            user.WalletId = 1;

            // Create the user with the specified password
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            // Assign the "Student" role to the newly created user
            await _userManager.AddToRoleAsync(user, "User");
            await _userManager.AddToRoleAsync(user, "Admin");

            // Generate JWT token for the user
            var wallet = new Wallet
            {
                Balance = 0, // Initial balance
                CreatedDate = DateTime.UtcNow,

            };

 

            // Return the authentication model
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = DateTime.Now.AddDays(30),
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = "",
                Name = user.Name,
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed
            };
        }


        public async Task<AuthModel> GetTokenAsync(LoginModel model)
        {
            var authmodel = new AuthModel();
            var user1 = CreateUser();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {

                authmodel.Message = "Email or Password is Incorrect";
                return authmodel;
            }
            user1 = await _Context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            var jwtSecurityToken = "";

            authmodel.IsAuthenticated = true;
            authmodel.Token = "";
            authmodel.Email = user1.UserName;
            authmodel.ImgUrl = user1.image;

            authmodel.Id = user1.Id;
            authmodel.Name = user1.Name;
            authmodel.ExpiredOn = DateTime.Now.AddDays(30);


            var userRoles = await _userManager.GetRolesAsync(user);
            authmodel.Roles = userRoles.ToList();
            return authmodel;
        }
        //public async Task<AuthModel> GetGoogleTokenAsync(string email)
        //{
        //    var authmodel = new AuthModel();
        //    var user = CreateUser();
        //    user = await _Context.Users.FirstOrDefaultAsync(u => u.Email == email);



        //    authmodel.IsAuthenticated = true;
        //    authmodel.Token = "";
        //    authmodel.Email = user.UserName;
        //    authmodel.ImgUrl = user.image;
        //    authmodel.Name = user.Name;
        //    authmodel.Id = user.Id;


        //    authmodel.Email = user.Email;
        //    authmodel.ExpiredOn = DateTime.Now.AddDays(30);


        //    var userRoles = await _userManager.GetRolesAsync(user);
        //    authmodel.Roles = userRoles.ToList();
        //    return authmodel;
        //}

        //public async Task<User> EditAsync(AuthModel customer)
        //{
        //    var user = await _Context.Users.FirstOrDefaultAsync(c => c.Email == customer.Email);
        //    user.Name = customer.Name;

        //    _Context.Users.Update(user);
        //    await _Context.SaveChangesAsync();
        //    return user;

        //}

        //    public async Task<string> AddRoleAsync(AddRoleModel model)
        //{
        //    var user =await _userManager.FindByIdAsync(model.UserId);
        //    if (user == null || !await _roleManager.RoleExistsAsync(model.Role) )
        //        return "Invalid User Id or Role";


        //    if (await _userManager.IsInRoleAsync(user, model.Role))
        //        return "User Already Exist in this Role";

        //    var result=await _userManager.AddToRoleAsync(user, model.Role);
        //    if (result.Succeeded)
        //        return string.Empty;

        //    return "Something went Wrong";

        //}






    }
}
