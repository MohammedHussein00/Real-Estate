using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using Music_Aditor.Services;
using Real_State.Data;
using Real_State.Models;
using Real_State.Services;
using Real_State.Tables;
using System.Reflection.Metadata;
using System;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;


namespace Real_State.Controllers
{
    public class UserController : Controller
    {
        private readonly IHostingEnvironment environment;
        private readonly ApplicationDbContext _Context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        private readonly IPropertyService _propertyService;
        private readonly IUserService _userService;


        public UserController(IAdminService adminService,IPropertyService propertyService, IUserService userService, IAuthService authService, IHostingEnvironment Environment, ApplicationDbContext Context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _Context = Context;
            environment = Environment;
            _authService = authService;
            _propertyService = propertyService;
            _userService = userService;
            _adminService = adminService;



            _signInManager = signInManager;
        }


        public async Task<ActionResult> Wallet()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId is null)
            {
                return RedirectToAction("Login", "Auth"); // Redirect to the login page
            }
            var user = await _Context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var wallet = await _Context.Wallets.Where(w=>w.Id==user.WalletId)
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync();

            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            var model = new WalletViewModel
            {
                Balance = wallet.Balance,
                Transactions = wallet.Transactions.Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Type = t.Type,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate
                }).ToList()
            };

            return View("Wallet", model); // Explicitly specify the view name
        }

        [HttpPost]
        public async Task<JsonResult> UpdateBalance(decimal amount)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _Context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (amount <= 0)
            {
                return Json(new { success = false, message = "Amount must be greater than 0." });
            }

            var wallet = await _Context.Wallets.Where(w => w.Id == user.WalletId).FirstOrDefaultAsync();

            if (wallet == null)
            {
                return Json(new { success = false, message = "Wallet not found." });
            }

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Type = "Charge",
                Amount = amount,
                Description = $"You have been charged your wallet with {amount} EGP on {DateTime.UtcNow}.",
                TransactionDate = DateTime.UtcNow
            };

            wallet.Balance += amount;

            _Context.Transactions.Add(transaction);
            _Context.Wallets.Update(wallet);
            await _Context.SaveChangesAsync();

            return Json(new { success = true, message = "Transaction completed successfully." });
        }

        public IActionResult Profile()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Fetch data from the database based on Id
            var profile = _Context.Users.Find(userId); // Replace with your data fetching logic

            if (profile == null)
            {
                return NotFound();
            }

            return View(new ProfileModel { Email=profile.Email,Name=profile.Name,Phone=profile.PhoneNumber,Location=profile.Location,Photo=null,
                GizaLocations = _adminService.ReadLocationsFromFile("giza_locations.bin"),
                CairoLocations = _adminService.ReadLocationsFromFile("cairo_locations.bin")

            });
        }


        // Handle profile update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([FromForm]ProfileModel model)
        {
         
                // Update the database
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;




                var user =await _Context.Users.FindAsync(userId); // Replace with your data fetching logic


                if (user is not null)
                {


                    if (model.Photo != null && model.Photo.Length > 0)
                    {
                        // Define user-specific folder path based on email
                        string folderPath = Path.Combine("wwwroot", "Users Images", user.Email);

                        // Ensure folder exists
                        if (Directory.Exists(folderPath))
                        {
                            // Remove all files in the folder
                            var files = Directory.GetFiles(folderPath);
                            foreach (var file in files)
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                        else
                        {
                            // Create the folder if it doesn't exist
                            Directory.CreateDirectory(folderPath);
                        }

                        // Save the new photo
                        string newFileName = $"{model.Photo.FileName}";
                        string filePath = Path.Combine(folderPath, newFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.Photo.CopyTo(stream);
                        }
                    user.image =$"/Users Images/{user.UserName}/{model.Photo.FileName}";


                    }
                    user.Name = model.Name;
                    user.Location = model.Location;
                    user.PhoneNumber = model.Phone;
                     _Context.Users.Update(user); // Replace with your database update logic
                    await _Context.SaveChangesAsync();
                    // Retrieve the current claims
                    // Retrieve the current user's claims
                    var currentClaims = User.Claims.ToList();

                    // Remove the current user's claims
                    var authenticationManager = HttpContext;
                    await authenticationManager.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Create a new list of claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim("ImgUrl", user.image ?? string.Empty) // Custom claim for ImgUrl
                    };

                    // Retrieve the user's roles
                    // Retrieve the user's roles asynchronously
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles != null && roles.Count > 0) // Use Count instead of Any for IList
                    {
                        // Add role claims
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }


                    // Create a new ClaimsIdentity
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Set authentication properties
                    var properties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true // Adjust based on application needs
                    };

                    // Sign in the user with the updated claims
                    await authenticationManager.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        properties
                    );
                }
                // Redirect to a success page or back to a list
                return RedirectToAction("Profile");
            

            
        }

        #region property section



        [HttpPost]
        public async Task<IActionResult> SaveProperty(int propertyId)
        {
            // Check if the user is logged in
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth"); // Redirect to the login page
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the user already saved this property
            var existingUserProperty = await _Context.UserProperties
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PropertyId == propertyId);

            if (existingUserProperty != null)
            {
                // If the property is saved, unsave it (remove from saved list)
                if (existingUserProperty.Type.HasFlag(RelationshipType.Saved))
                {
                    existingUserProperty.Type &= ~RelationshipType.Saved; // Remove "Saved" flag
                    _Context.UserProperties.Update(existingUserProperty);
                    await _Context.SaveChangesAsync();

                    return Json(new { success = true, message = "Property unsaved successfully!" });
                }
                else
                {
                    // If it's not saved, save it
                    existingUserProperty.Type |= RelationshipType.Saved; // Add "Saved" flag
                    _Context.UserProperties.Update(existingUserProperty);
                    await _Context.SaveChangesAsync();

                    return Json(new { success = true, message = "Property saved successfully!" });
                }
            }
            else
            {
                // If the property was never saved before, save it now
                var userProperty = new UserProperty
                {
                    PropertyId = propertyId,
                    UserId = userId,
                    Type = RelationshipType.Saved
                };

                _Context.UserProperties.Add(userProperty);
                await _Context.SaveChangesAsync();

                return Json(new { success = true, message = "Property saved successfully!" });
            }
        }


        public async Task<IActionResult> SavedProperties()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth"); // Redirect to the login page
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var savedProperties =await _propertyService.GetSavedProperties(userId);  // Fetch saved properties

            return View(savedProperties);
        }

        // Action to remove a single saved property
        [HttpPost]
        public async Task<IActionResult> RemoveSavedProperty(int propertyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingUserProperty = await _Context.UserProperties
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PropertyId == propertyId);

            if (existingUserProperty != null && existingUserProperty.Type.HasFlag(RelationshipType.Saved))
            {
                existingUserProperty.Type &= ~RelationshipType.Saved;

                if (existingUserProperty.Type == 0)
                {
                    _Context.UserProperties.Remove(existingUserProperty);
                }
                else
                {
                    _Context.UserProperties.Update(existingUserProperty);
                }

                await _Context.SaveChangesAsync();
                return Json(new { success = true, message = "Property removed successfully!" });
            }

            return Json(new { success = false, message = "Property was not found or already removed." });
        }

        // Action to remove all saved properties
        [HttpPost]
        public async Task<IActionResult> RemoveAllSavedHomes()
        {
            // Get the logged-in user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated
            }

            try
            {
                // Fetch all saved properties for the user and remove them
                var savedProperties = await _Context.UserProperties
                    .Where(up => up.UserId == userId && up.Type.HasFlag(RelationshipType.Saved))
                    .ToListAsync();

                if (savedProperties.Any())
                {
                    // Update or remove relationships as needed
                    foreach (var property in savedProperties)
                    {
                        property.Type &= ~RelationshipType.Saved; // Remove "Saved" flag

                        if (property.Type == 0) // If no other flags are set, remove the relationship
                        {
                            _Context.UserProperties.Remove(property);
                        }
                        else
                        {
                            _Context.UserProperties.Update(property);
                        }
                    }

                    await _Context.SaveChangesAsync();

                    return RedirectToAction("SavedProperties", "User"); // Redirect to the login page
                }
                else
                {
                    return RedirectToAction("SavedProperties", "User"); // Redirect to the login page
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return RedirectToAction("SavedProperties", "User"); // Redirect to the login page
            }

            return RedirectToAction("SavedProperties", "User"); // Redirect to the login page
        }

        #endregion


        [HttpGet]
        public async Task<IActionResult> GetNotificationChat()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Return a JSON response indicating the user is not authenticated
                return RedirectToAction("Login", "Auth"); // Redirect to the login page
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Fetch notifications for the logged-in user
            var notifications = await _Context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            // Return the notifications as JSON
            return Json(new { IsAuthenticated = true, Notifications = notifications });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Mark notifications as read for the current user
                var notifications = await _Context.Notifications.Where(n=>n.UserId==userId&& !n.IsRead).ToListAsync();
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                // Update the notifications in the database
                 _Context.Notifications.UpdateRange(notifications);
                await _Context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to change your password." });
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var changePasswordResult = await _userService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (changePasswordResult)
            {
                return Json(new { success = true, message = "Your password has been changed successfully." });
            }
            else
            {
                return Json(new { success = false, message = "An error occurred while changing the password. Please ensure your old password is correct." });
            }
        }

        #region Appointments get all and cancel

        [Route("User/UserAppointments")]
        [HttpGet]
        public IActionResult UserAppointments()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth"); // Redirect to the login page
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var appointments = _Context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Property)
                .ToList();

            return View(appointments);
        }

        [Route("User/CancelAppointment/{appointmentId:int}")]
        [HttpPost]
        public JsonResult CancelAppointment(int appointmentId)
        {
            try
            {
                var appointment = _Context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found." });
                }

                // Check if the appointment is within the one-hour cancellation window
                if (DateTime.Now - appointment.AppointmentDate > TimeSpan.FromHours(1))
                {
                    return Json(new { success = false, message = "Appointments can only be canceled within one hour of booking." });
                }

                _Context.Appointments.Remove(appointment);
                _Context.SaveChanges();

                return Json(new { success = true, message = "Appointment canceled successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while canceling the appointment.", error = ex.Message });
            }
        }

        #endregion
    }



}
