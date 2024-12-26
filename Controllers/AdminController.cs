using Microsoft.AspNetCore.Mvc;
using Real_State.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Music_Aditor.Services;
using Microsoft.AspNetCore.Authorization;
using Real_State.Models;
using Real_State.Tables;
using Newtonsoft.Json;
using System.IO;
using Real_State.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using System;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace Real_State.Controllers
{
    [Authorize(Roles = "SuberAdmin,Admin")]
    public class AdminController : Controller
    {
        private readonly IHostingEnvironment environment;
        private readonly ApplicationDbContext _Context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        private readonly IPropertyService _propertyService;
        private readonly IUserService _userService;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(IAuthService authService, IHostingEnvironment Environment,
            ApplicationDbContext Context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IPropertyService propertyService,
            IAdminService adminService,
            IUserService userService,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _Context = Context;
            _userService = userService;
            _roleManager = roleManager;

            environment = Environment;
            _authService = authService;
            _adminService = adminService;
            _propertyService = propertyService;


            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<ActionResult> Dashboard()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var analytics = new DashboardAnalytics
            {
                TotalUsers = users.Except(admins).Count(),
                TotalAdmins = admins.Count,
                TotalProperties = _Context.Properties.Count(),
                PropertiesSold = _Context.Properties.Count(p => p.IsSold),
                PropertiesUnsold = _Context.Properties.Count(p => !p.IsSold),
                TotalRevenue = _Context.Properties.Where(p => p.IsSold).Sum(p => (decimal?)p.Price) ?? 0, // Default to 0 if no sold properties


                RevenueToday = _Context.Transactions.Where(p => p.TransactionDate==DateTime.UtcNow&&p.Description.Contains("deducted from your wallet as a result of the purchase this property")).Sum(p => (decimal?)p.Amount) ?? 0,

                SalesToday = _Context.Transactions
                .Where(p => p.TransactionDate.Date == DateTime.UtcNow.Date &&
                            p.Description.Contains("deducted from your wallet as a result of the purchase this property"))
                .Count(),

                TotalSales = _Context.Transactions
                .Where(p =>  p.Description.Contains("deducted from your wallet as a result of the purchase this property"))
                .Count()






            };

            var walletTransactions = await _Context.Transactions
                .Where(t => t.WalletId == 1)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.WalletTransactions = JsonConvert.SerializeObject(walletTransactions);
            return View(analytics);
        }

        public async Task<IActionResult> Admins()
        {
            var adminsModel = new List<AllAdminsModel>();

            // Check if the Admin or SuperAdmin roles exist
            var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            var superAdminRoleExists = await _roleManager.RoleExistsAsync("SuperAdmin");

            if (adminRoleExists || superAdminRoleExists)
            {
                // Get all users in the Admin role
                var admins = new List<IdentityUser>();

                if (adminRoleExists)
                {
                    admins.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));
                }

                // Get all users in the SuperAdmin role
                if (superAdminRoleExists)
                {
                    admins.AddRange(await _userManager.GetUsersInRoleAsync("SuperAdmin"));
                }

                // Map IdentityUser to AllAdminsModel
                adminsModel = admins.Select(admin => new AllAdminsModel
                {
                    Id = admin.Id, // Ensure Id is mapped as a string
                    Name = (_Context.Users.Where(a => a.Id == admin.Id).FirstOrDefault())?.Name,
                    Email = admin.Email,
                    Phone = admin.PhoneNumber,
                    ImageUrl = (_Context.Users.Where(a => a.Id == admin.Id).FirstOrDefault())?.image
                }).ToList();
            }

            return View(adminsModel); // Return the model to the view
        }

        public async Task<IActionResult> Users()
        {
            var usersModel = new List<AllAdminsModel>();

            // Check if the User, Admin, or SuperAdmin roles exist
            var userRoleExists = await _roleManager.RoleExistsAsync("User");
            var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            var superAdminRoleExists = await _roleManager.RoleExistsAsync("SuperAdmin");

            if (userRoleExists || adminRoleExists || superAdminRoleExists)
            {
                // Use HashSet for efficient set operations
                var allUsers = new HashSet<IdentityUser>();

                // Add users in the User role
                if (userRoleExists)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync("User");
                    allUsers.UnionWith(usersInRole);
                }

                // Exclude users in the Admin role
                if (adminRoleExists)
                {
                    var adminsInRole = await _userManager.GetUsersInRoleAsync("Admin");
                    allUsers.ExceptWith(adminsInRole);
                }

                // Exclude users in the SuperAdmin role
                if (superAdminRoleExists)
                {
                    var superAdminsInRole = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                    allUsers.ExceptWith(superAdminsInRole);
                }

                // Optimize database access by retrieving all necessary user details in one query
                var userIds = allUsers.Select(user => user.Id).ToList();
                var userDetails = _Context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionary(u => u.Id);

                // Map IdentityUser to AllAdminsModel
                usersModel = allUsers.Select(user => new AllAdminsModel
                {
                    Id = user.Id,
                    Name = userDetails.ContainsKey(user.Id) ? userDetails[user.Id].Name : null,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    ImageUrl = userDetails.ContainsKey(user.Id) ? userDetails[user.Id].image : null
                }).ToList();
            }

            // Return the list to the view
            return View(usersModel);
        }


        public ActionResult AddAdmin()
        {

            var model = new RegisterViewModel();
            // Optionally set the Message property here if needed
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddAdmin(RegisterModel model)
        {
            var result = await _authService.RegisterAdminAsync(model);

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError("", result.Message);

                return View(new RegisterViewModel { Email = model.Email, Name = model.Name, Message = result.Message, Password = model.Password, Phone = model.Phone });
            }

            return View(new RegisterViewModel { Email = model.Email, Name = model.Name, Message = result.Message, Password = model.Password, Phone = model.Phone });

        }

        public async Task<ActionResult> GetAllProperties(
            int page = 1, int pageSize = 5, string location = null, string purpose = null,
            double minPrice = 40000, double maxPrice = 1000000, double minSpace = 40,
            double maxSpace = 3000, int bedrooms = 0, int bathrooms = 0, string type = "Select Property Type",
            string sort = "Sort By")
        {
            if (string.IsNullOrEmpty(purpose))
            {
                purpose = "All"; // Default to "All"
            }

            // Retrieve all properties as a list
            var allProperties = await _Context.Properties.Where(p => p.IsSold == false).ToListAsync();

            // Apply filtering, sorting, and pagination to the properties
            var properties = await GetProperties(allProperties, page, pageSize, location, purpose, minPrice, maxPrice, minSpace, maxSpace, bedrooms, bathrooms, type, sort);

            // Fetch the total number of properties matching the criteria
            var totalProperties = await _propertyService.GetAllProperties(allProperties, location, purpose, minPrice, maxPrice, minSpace, maxSpace, bedrooms, bathrooms, type, sort);
            var totalPages = (int)Math.Ceiling(totalProperties.Count() / (double)pageSize);
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Initialize list for saved property status and agents
            var propertiesSavedStatus = new List<PropertiesSavedStatus>();
            var agents = new List<AgentModel>();

            // Use recursion to get properties saved status and agents
            if (!string.IsNullOrEmpty(userId))
            {
                propertiesSavedStatus = await _adminService.GetPropertiesSavedStatusRecursive(properties, userId, 0);
            }

            agents = await _adminService.GetAgentsRecursive(properties, 0);

            var model = new PropertySearchModel
            {
                Properties = properties,
                CurrentPage = page,
                TotalPages = totalPages,
                SelectedLocation = location,
                Purpose = purpose,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MaxSpace = maxSpace,
                MinSpace = minSpace,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                Type = type,
                Sort = sort,
                PropertiesSavedStatus = propertiesSavedStatus,
                AgentModels = agents
            };

            return View(model);
        }







        public async Task<ActionResult> SoldProperties(int page = 1, int pageSize = 5, string location = null, string purpose = null, double minPrice = 40000, double maxPrice = 1000000,
            double minSpace = 40, double maxSpace = 3000, int bedrooms = 0, int bathrooms = 0, string type = "Select Property Type", string sort = "Sort By"
        )
        {
            if (string.IsNullOrEmpty(purpose))
            {
                purpose = "All"; // Default to "All"
            }

            // Retrieve all properties as a list
            var allProperties = await _Context.Properties.Where(p=>p.IsSold==true).ToListAsync();

            // Apply filtering, sorting, and pagination to the properties
            var properties = await GetProperties(allProperties, page, pageSize, location, purpose, minPrice, maxPrice, minSpace, maxSpace, bedrooms, bathrooms, type, sort);

            // Fetch the total number of properties matching the criteria
            var totalProperties = await _propertyService.GetAllProperties(allProperties, location, purpose, minPrice, maxPrice, minSpace, maxSpace, bedrooms, bathrooms, type, sort);
            var totalPages = (int)Math.Ceiling(totalProperties.Count() / (double)pageSize);
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Initialize list for saved property status
            var propertiesSavedStatus = new List<PropertiesSavedStatus>();
            var agents = new List<AgentModel>();
            foreach (var property in properties)
            {
                var agentId = (await _Context.UserProperties
                     .Where(p => p.PropertyId == property.Id &&
                    ((p.Type & RelationshipType.Created) == RelationshipType.Created ||
                     (p.Type & RelationshipType.Viewed) == RelationshipType.Viewed))
                        .FirstOrDefaultAsync())?.UserId;

                var agent = await _Context.Users.Where(u => u.Id == agentId).FirstOrDefaultAsync();
                agents.Add(new AgentModel { Id = agent.Id, Name = agent.Name, Email = agent.Email, Image = agent.image, Phone = agent.PhoneNumber });
            }
            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var property in properties)
                {
                    var existingUserProperty = await _Context.UserProperties
                        .FirstOrDefaultAsync(up => up.UserId == userId && up.PropertyId == property.Id);

                    if (existingUserProperty != null)
                    {
                        // If the property is saved, mark as true, else false
                        propertiesSavedStatus.Add(new PropertiesSavedStatus { Id = property.Id, Status = existingUserProperty.Type.HasFlag(RelationshipType.Saved) });
                    }
                    else
                    {
                        // If no property saved status found, mark as false
                        propertiesSavedStatus.Add(new PropertiesSavedStatus { Id = property.Id, Status = false });
                    }
                }
            }
            var model = new PropertySearchModel
            {
                Properties = properties,
                CurrentPage = page,
                TotalPages = totalPages,
                SelectedLocation = location,
                Purpose = purpose,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MaxSpace = maxSpace,
                MinSpace = minSpace,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                Type = type,
                Sort = sort,
                PropertiesSavedStatus = propertiesSavedStatus,
                AgentModels = agents
            };

            return View(model);
        }


        private async Task<List<Tables.Property>> GetProperties(List<Tables.Property> properties, int page, int pageSize, string location, string purpose, double minPrice, double maxPrice,
    double minSpace, double maxSpace, int bedrooms, int bathrooms, string type, string sort)
        {
            // Apply filters and sorting using the _propertyService
            var allProperties = await _propertyService.GetAllProperties(properties, location, purpose, minPrice, maxPrice, minSpace, maxSpace, bedrooms, bathrooms, type, sort);

            // Apply pagination
            return allProperties
                .Skip((page - 1) * pageSize)  // Skip the number of items from previous pages
                .Take(pageSize)               // Take the number of items for the current page
                .ToList();
        }
        public ActionResult AddProperty()
        {
            return View(new AddPropertyModel {
                GizaLocations = _adminService.ReadLocationsFromFile("giza_locations.bin"),
                CairoLocations = _adminService.ReadLocationsFromFile("cairo_locations.bin")
            });
        }

        [HttpPost]
        public async Task<ActionResult> AddProperty([FromForm] AddPropertyModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _propertyService.AddProperty(model);
                return RedirectToAction("PropertyDetails", new { id = result });
            }
            catch (Exception ex)
            {
                // Log the exception (use a logger if available)
                Console.WriteLine($"Error in AddProperty Controller: {ex.Message}");

                // Optionally, add a model error to display a message to the user
                ModelState.AddModelError("", "An error occurred while adding the property. Please try again.");

                return RedirectToAction();
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveProperty(int propertyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth"); // Redirect to login if not authenticated
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var (success, message) = await _adminService.TogglePropertySaveStatusAsync(userId, propertyId);

            return Json(new { success, message });
        }
        [Route("Admin/BookedAppointments/{propertyId:int}/{appointmentType}")]
        public async Task<IActionResult> BookedAppointments(int propertyId, string appointmentType)
        {
            var property = await _Context.Properties
                .Where(p => p.Id == propertyId)
                .FirstOrDefaultAsync();


            if (property == null)
            {
                return NotFound();
            }
            if (appointmentType.ToLower() == "all")
            {
                var appointments = await _Context.Appointments
                .Where(a => a.PropertyId == propertyId)
                .Include(a => a.User)
                .ToListAsync();

                ViewBag.PropertyTitle = property.Title;

                return View(appointments);
            }
            else
            {
                var appointments = await _Context.Appointments
                .Where(a => a.PropertyId == propertyId&&a.AppointmentType.ToLower()=="buy")
                .Include(a => a.User)
                .ToListAsync();

                ViewBag.PropertyTitle = property.Title;
                ViewBag.IsSold = property.IsSold;

                return View(appointments);
            }
        }
            
        [HttpPost]
        [Route("api/properties/{propertyId}/{userEmail}/mark-sold")]
        public async Task<IActionResult> MarkPropertyAsSold(int propertyId, string userEmail)
        {
            var property = await _Context.Properties.FindAsync(propertyId);
            var currentUser=_Context.Users.Where(u=>u.UserName==userEmail).FirstOrDefault();
            if (property == null)
            {
                return NotFound("Property not found.");
            }
           var wallet= _Context.Wallets.Where(w => w.Id == (_Context.Users.Where(u => u.UserName == userEmail).FirstOrDefault().WalletId)).FirstOrDefault();
            if (wallet is not null &&wallet.Balance >= property.Price)
            {
                wallet.Balance -= property.Price ;
                _Context.Wallets.Update(wallet);

                await SendNotificationToUser(currentUser, $"Property Price deducted from your wallet as a result of the purchase Property {property.Title} ");
                await _userService.SendEmail("test", currentUser.UserName, "deducted from your wallet", $"{property.Price} Property Price deducted from your wallet as a result of the buy property {property.Title}<br/><br/><a href='https://localhost:7199/User/Wallet'>Go to check your wallet</a>");
                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = "Dispount",
                    Amount = property.Price ,
                    Description = $"price of Property {property.Title} deducted from your wallet as a result of the purchase this property",
                    TransactionDate = DateTime.UtcNow
                };


                _Context.Transactions.Add(transaction);
                var walletAdmin = _Context.Wallets.Where(w => w.Id == 1).FirstOrDefault();
                walletAdmin.Balance += property.Price ;
                _Context.Wallets.Update(walletAdmin);
                var transactionAdmin = new Transaction
                {
                    WalletId = walletAdmin.Id,
                    Type = "Withdrawal",
                    Amount = property.Price * 0.01m,
                    Description = $"price of Property {property.Title} added to the wallet as a result of the purchase this Property. ",
                    TransactionDate = DateTime.UtcNow
                };


                _Context.Transactions.Add(transactionAdmin);
                await _Context.SaveChangesAsync();
            }

            // Get all users who booked an appointment for this property

            var bookedUsers = await _Context.Appointments
            .Where(a => a.PropertyId == propertyId)
            .Select(a => a.User)
            .ToListAsync();
            // Send notifications to all users
            if (bookedUsers.Any())
            {

                foreach (var user in bookedUsers)
                {

                    // Update the IsSold property
                    property.IsSold = true;
                    property.SoldDescription = $"Sold to {user.Name} with email {user.UserName}";
            _Context.Properties.Update(property);
                    if (user.UserName != userEmail)
                    {
                        await SendNotificationToUser(user, $"Property {property.Title} that booked is Sold to {_Context.Users.Where(u => u.UserName == userEmail).FirstOrDefault()} with email {userEmail}");
                            await _userService.SendEmail("test", currentUser.UserName, $"Property {property.Title} that booked is Sold", $"Property {property.Title} that booked is Sold to {_Context.Users.Where(u => u.UserName == userEmail).FirstOrDefault()} with email {userEmail}");
                    }
                }
            }

            await _Context.SaveChangesAsync();

            return Ok(new { message = "Property sold successfully." });
        }


        [Route("Admin/PropertyDetails/{propertyId:int}")]
        public async Task<ActionResult> PropertyDetails(int propertyId)
        {
            // Retrieve the property details
            var property = await _Context.Properties
                .Where(p => p.Id == propertyId)
                .FirstOrDefaultAsync();

            if (property == null)
            {
                return NotFound(); // Property not found
            }

            // Retrieve additional images
            var additionalImages = await _Context.PropertyImages
                .Where(i => i.PropertyId == propertyId)
                .Select(i => i.ImagePath)
                .ToListAsync();

            // Get the agent associated with the property
            var agentId = await _Context.UserProperties
                .Where(p => p.PropertyId == propertyId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            var agent = agentId != null
                ? await _Context.Users.FirstOrDefaultAsync(u => u.Id == agentId)
                : null;

            // Get current user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isSaved = false;

            if (!string.IsNullOrEmpty(userId))
            {
                // Retrieve the existing UserProperty relationship if any
                var userProperty = await _Context.UserProperties
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PropertyId == propertyId);

                if (userProperty != null)
                {
                    // Check if the property is saved by the user
                    isSaved = userProperty.Type.HasFlag(RelationshipType.Saved);

                    // Update the relationship to include 'Viewed' if not already present
                    if (!userProperty.Type.HasFlag(RelationshipType.Viewed))
                    {
                        userProperty.Type |= RelationshipType.Viewed;
                        _Context.UserProperties.Update(userProperty);
                        await _Context.SaveChangesAsync();
                    }
                }
                else
                {
                    // If this is the user's first interaction with the property, create a new entry
                    var newUserProperty = new UserProperty
                    {
                        UserId = userId,
                        PropertyId = propertyId,
                        Type = RelationshipType.Viewed
                    };

                    await _Context.UserProperties.AddAsync(newUserProperty);
                    await _Context.SaveChangesAsync();
                }
            }

            // Calculate total views for the property
            var totalViews = await _Context.UserProperties
                .Where(up => up.PropertyId == propertyId && up.Type.HasFlag(RelationshipType.Viewed))
                .CountAsync();

            // Map data to PropertyDetailsModel
            var model = new PropertyDetailsModel
            {
                Id = property.Id,
                Title = property.Title,
                Location = property.Location,
                Space = property.Space,
                Purpose = property.Purpose,
                Description = property.Description,
                Type = property.Type,
                Date = property.Date,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Sapce = property.Sapce, // Typo preserved if intentional; otherwise change to Space
                Price = property.Price,
                MainImagePath = property.MainImagePath,
                AgentId = agent?.Id,
                AgentName = agent?.Name,
                AgentEmail = agent?.Email,
                AgentPhone = agent?.PhoneNumber,
                AgentImagePath = agent?.image,
                AdditionalImages = additionalImages,
                IsSold = property.IsSold,
                TotalViews = totalViews,
                IsSaved = isSaved
            };

            ViewBag.IsSaved = isSaved; // Pass saved status to the view

            // Return the view with the model
            return View(model);
        }

        private async Task SendNotificationToUser(User user,string description)
        {
            // This is a placeholder for sending notifications to the user
            // Depending on your notification system, you might send an email, push notification, or another type of notification

            var notification = new Notification
            {
                UserId = user.Id,
                Content = description,
                CreatedAt = DateTime.UtcNow
            };

            _Context.Notifications.Add(notification);
            await _Context.SaveChangesAsync();

            // Optionally, you can send an email or other notification method to the user
        }

        #region Wallet Section
        public async Task<ActionResult> WalletTransaction()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
            {
                return RedirectToAction("Login", "Auth"); // Redirect to the login page
            }
            var user = await _Context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var wallet = await _Context.Wallets.Where(w => w.Id == user.WalletId)
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

            return View("WalletTransaction", model); // Explicitly specify the view name
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

        #endregion

        public IActionResult Profile()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Fetch data from the database based on Id
            var profile = _Context.Users.Find(userId); // Replace with your data fetching logic

            if (profile == null)
            {
                return NotFound();
            }

            return View(new ProfileModel
            {
                Email = profile.Email,
                Name = profile.Name,
                Phone = profile.PhoneNumber,
                Location = profile.Location,
                Photo = null,
                GizaLocations = _adminService.ReadLocationsFromFile("giza_locations.bin"),
                CairoLocations = _adminService.ReadLocationsFromFile("cairo_locations.bin")

            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([FromForm] ProfileModel model)
        {
         
                // Update the database
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;




                var user = await _Context.Users.FindAsync(userId); // Replace with your data fetching logic


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
                        user.image = $"/Users Images/{user.UserName}/{model.Photo.FileName}";


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

        [HttpGet("Admin/EditProperty/{propertyId}")]
        public async Task<IActionResult> EditProperty(int propertyId)
        {
            // Fetch the property to update based on the provided propertyId
            var property = await _Context.Properties
                .Include(p => p.PropertyImages) // Include property images, if needed
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
            {
                return NotFound("Property not found.");
            }
            var model = new UpdatePropertyModel
            {
               Id=propertyId,
                Title = property.Title,
                Location = property.Location,
                Description = property.Description,
                Type = property.Type,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Space = property.Space,
                Price = property.Price,
                GizaLocations = _adminService.ReadLocationsFromFile("giza_locations.bin"),
                CairoLocations = _adminService.ReadLocationsFromFile("cairo_locations.bin"),
                Purpose = property.Purpose,
                MainImageURL=property.MainImagePath,
                AdditionalImagesURL=property?.PropertyImages.Select(i=>i.ImagePath).ToList()

            };

            return View(model);
        }
        [HttpPost("Admin/EditProperty/{propertyId}")]
        public async Task<IActionResult> EditProperty(int propertyId, [FromForm] UpdatePropertyModel model)
        { 
            model.AdditionalImagesURL = model.AdditionalImagesURL.FirstOrDefault().Split(',').ToList();
            var result = await _propertyService.UpdateProperty(propertyId,model);

            if (result)
            {
                return RedirectToAction("PropertyDetails", new { propertyId = propertyId });
            }
            else
            {
                return RedirectToAction("PropertyDetails", new { propertyId = propertyId });
            }
        }


        [HttpDelete("remove/{propertyId}")]
        public async Task<IActionResult> RemoveProperty(int propertyId)
        {
            try
            {
                bool result = await _propertyService.RemoveProperty(propertyId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return Ok(new { success = false });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Location Management

        public IActionResult Locations()
        {
            var model = new LocationViewModel
            {
                CairoLocations = _adminService.ReadLocationsFromFile("cairo_locations.bin")
                    .Select(location => new LocationItem
                    {
                        Name = location,
                        IsUsed = _adminService.IsLocationUsedByAnyUser(location)
                    })
                    .ToList(),
                GizaLocations = _adminService.ReadLocationsFromFile("giza_locations.bin")
                    .Select(location => new LocationItem
                    {
                        Name = location,
                        IsUsed = _adminService.IsLocationUsedByAnyUser(location)
                    })
                    .ToList()
            };

            return View(model);
        }



        [HttpPost]
        public IActionResult SaveLocations([FromBody] LocationRequestModel request)
        {
            try
            {
                // Save Cairo locations and capture the message
                var cairoMessage = _adminService.SaveLocationsToFile(request.CairoLocations, "cairo_locations.bin");

                // Save Giza locations and capture the message
                var gizaMessage = _adminService.SaveLocationsToFile(request.GizaLocations, "giza_locations.bin");

                // Combine messages for the response
                var responseMessage = $"{cairoMessage} {gizaMessage}";

                return Json(new { success = true, message = responseMessage.Trim() });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Error saving locations: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while saving locations." });
            }
        }


        #endregion

    }
}

