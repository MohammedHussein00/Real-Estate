using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Music_Aditor.Services;
using Real_State.Data;
using Real_State.Models;
using Real_State.Services;
using Real_State.Tables;
using Real_State.ViewModels;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace Real_State.Controllers
{
    public class HomePageController : Controller
    {
        private readonly IHostingEnvironment environment;
        private readonly ApplicationDbContext _Context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthService _authService;
        private readonly IPropertyService _propertyService;


        public HomePageController(IAuthService authService, IHostingEnvironment Environment,
            ApplicationDbContext Context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IPropertyService propertyService)
        {
            _userManager = userManager;
            _Context = Context;
            environment = Environment;
            _authService = authService;
            _propertyService = propertyService;


            _signInManager = signInManager;
        }


        public IActionResult Home(RegisterModel model)
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        [Route("HomePage/PropertyDetails/{propertyId:int}")]
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
                IsSaved= isSaved
            };

            ViewBag.IsSaved = isSaved; // Pass saved status to the view

            // Return the view with the model
            return View(model);
        }

        public async Task<ActionResult> PropertyList(int page = 1, int pageSize = 5, string location = null, string purpose = null, double minPrice = 40000, double maxPrice = 1000000,
            double minSpace = 40, double maxSpace = 3000, int bedrooms = 0, int bathrooms = 0, string type = "Select Property Type", string sort = "Sort By"
        )
        {
            if (string.IsNullOrEmpty(purpose))
            {
                purpose = "All"; // Default to "All"
            }

            // Retrieve all properties as a list
            var allProperties = await _Context.Properties.ToListAsync();

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
                PropertiesSavedStatus= propertiesSavedStatus,
                AgentModels= agents
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

        [Authorize]
        [Route("HomePage/BookAppointment/{propertyId:int}/{appointmentType}")]
        [HttpGet]
        public IActionResult BookAppointment(int propertyId, string appointmentType)
        {
            var property = _Context.Properties.FirstOrDefault(p => p.Id == propertyId);
            var appointments = _Context.Appointments
                .Where(a => a.PropertyId == propertyId && a.AppointmentType == appointmentType)
                .ToList();

            var viewModel = new PropertyAppointmentViewModel
            {
                Property = property,
                Appointments = appointments,
                AppointmentType = appointmentType
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("HomePage/BookAppointment/{propertyId:int}/{appointmentType}")]
        public JsonResult BookAppointment(string UserId, int propertyId, string date, string time, string appointmentType)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new
                    {
                        success = false,
                        message = "You need to log in."
                    });
                }

                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Parse date and time
                DateTime appointmentDate;
                TimeSpan appointmentTime;

                try
                {
                    appointmentDate = DateTime.Parse(date);
                    appointmentTime = TimeSpan.Parse(time);
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid date or time format."
                    });
                }

                DateTime appointmentDateTime = appointmentDate.Add(appointmentTime);

                // Check if the appointment time is in the past
                if (appointmentDateTime <= DateTime.Now)
                {
                    return Json(new
                    {
                        success = false,
                        message = "The selected time is in the past. Please choose a valid future time slot."
                    });
                }

                // Check for an existing appointment of type "view"
                var existingViewAppointment = _Context.Appointments.FirstOrDefault(a =>
                    a.UserId == userId && a.PropertyId == propertyId && a.AppointmentType.ToLower() == "view");

                if (appointmentType.ToLower() == "buy" && existingViewAppointment != null)
                {
                    // Ensure the "buy" appointment time is after the "view" appointment time
                    DateTime existingViewDateTime = existingViewAppointment.AppointmentDate.Add(existingViewAppointment.AppointmentTime);
                    if (appointmentDateTime <= existingViewDateTime)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "The appointment time for buying must be after the time of your viewing appointment."
                        });
                    }
                }

                // Check for an existing appointment of the same type
                var existingAppointment = _Context.Appointments.FirstOrDefault(a =>
                    a.UserId == userId && a.PropertyId == propertyId && a.AppointmentType.ToLower() == appointmentType.ToLower());

                if (existingAppointment != null)
                {
                    // Update the existing appointment
                    existingAppointment.AppointmentDate = appointmentDateTime;
                    existingAppointment.AppointmentTime = appointmentTime;
                    _Context.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "Your appointment has been updated successfully!"
                    });
                }

                // Check wallet balance for 'buy' appointments
                decimal walletBalance = 0;
                var userWallet = _Context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.WalletId)
                    .FirstOrDefault();

                if (userWallet != null)
                {
                    walletBalance = _Context.Wallets
                        .Where(w => w.Id == userWallet)
                        .Select(w => w.Balance)
                        .FirstOrDefault();
                }

                var propertyPrice = _Context.Properties
                    .Where(p => p.Id == propertyId)
                    .Select(p => p.Price)
                    .FirstOrDefault();

                if (appointmentType.ToLower() == "buy" && propertyPrice / 100 > walletBalance)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Your wallet balance is insufficient. Please recharge your wallet to enable buying the property."
                    });
                }

                // Create a new appointment
                var newAppointment = new Appointment
                {
                    UserId = userId,
                    PropertyId = propertyId,
                    AppointmentTime = appointmentTime,
                    AppointmentType = appointmentType,
                    AppointmentDate = appointmentDateTime
                };

                _Context.Appointments.Add(newAppointment);
                _Context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Appointment booked successfully!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while booking the appointment. Try again later.",
                    error = ex.Message
                });
            }
        }


    }

    public class PropertySearchModel
    {
        public List<Tables.Property> Properties { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string SelectedLocation { get; set; }
        public string Purpose { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public double MinSpace { get; set; }
        public double MaxSpace { get; set; }
        public String Type { get; set; }
        public String Sort { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public List<PropertiesSavedStatus> PropertiesSavedStatus { get; set; }
        public List<AgentModel> AgentModels { get; set; }


    }

    public class PropertiesSavedStatus
    {
        public int Id { get; set; }
        public bool Status { get; set; }

    }


    public class AgentModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string Image { get; set; }
    }

}
