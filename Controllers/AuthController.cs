using Microsoft.AspNetCore.Mvc;
using Real_State.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Music_Aditor.Services;


using Real_State.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Real_State.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHostingEnvironment environment;
        private readonly ApplicationDbContext _Context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IAuthService _authService;

        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService,IHostingEnvironment Environment, ApplicationDbContext Context, UserManager<IdentityUser> userManager, IUserStore<IdentityUser> userStore, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _Context = Context;
            _userStore = userStore;
            environment = Environment;
            _authService = authService;


            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }





        public ActionResult Unuthorized()
        {

            // Optionally set the Message property here if needed
            return View();
        }       
        public ActionResult Register()
        {

            var model = new RegisterViewModel();
            // Optionally set the Message property here if needed
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            var result = await _authService.RegisterUserAsync(model);

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError("", result.Message); 

                return View(new RegisterViewModel { Email=model.Email,Name=model.Name,Message=result.Message,Password=model.Password,Phone=model.Phone}); 
            }
            List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, result.Id,result.ImgUrl),
                    new Claim(ClaimTypes.Email, result.Email),
                    new Claim(ClaimTypes.Role, result.Roles[0]), 
                    new Claim(ClaimTypes.Name, result.Name),
                        new Claim("ImgUrl", result.ImgUrl ?? string.Empty) // Assuming ImgUrl is in result

                };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = result.IsAuthenticated
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

            return RedirectToAction("Home", "HomePage");

        }

        public IActionResult Login()
        {

            var model = new LoginModel();
            // Optionally set the Message property here if needed
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {

            var result = await _authService.GetTokenAsync(model);

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError("", result.Message);

                return View(new LoginModel { Email = model.Email, Message = result.Message, Password = model.Password });
            }
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Id),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Name, result.Name),
                new Claim("ImgUrl", result.ImgUrl ?? string.Empty) // Custom claim for ImgUrl
            };

            // Add role claims separately
            foreach (var role in result.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = result.IsAuthenticated
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
            if(result.Roles.Count()>1)
            return RedirectToAction("Dashboard", "Admin");
            else
            return RedirectToAction("Home", "HomePage");
        }

        public ActionResult SaveUserDataInLocalStorage(RegisterModel model)
        {
            //var login = _Context.Users.FirstOrDefault(m => m.Email == sign.Email && m.Password == sign.Password);
            //if (login == null)
            //return Problem("error");
            //else

            return RedirectToAction("Home", "HomePage", model);

        }



        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the home page or login page after logging out
            return RedirectToAction("Home", "HomePage");
        }
        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

    }
}
