using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Real_State.Controllers;
using Real_State.Data;
using Real_State.Models;
using Real_State.Tables;
using System.Security.Claims;
using System.Security.Policy;


namespace Real_State.Services
{
    public class PropertyService:IPropertyService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _Context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;


        public PropertyService(UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor
            , RoleManager<IdentityRole> roleManager
            , ApplicationDbContext Context,
            IUserService userService
            )
      
        {
            _Context = Context;
            _userService = userService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
            _userService = userService;
        }
        public async Task<int> AddProperty(AddPropertyModel model)
        {
            try
            {
                // 1. Create a new property object from the model data
                var newProperty = new Tables.Property
                {
                    Title = model.Title,
                    Location = model.Location,
                    Description = model.Description,
                    Type = model.Type,
                    Bedrooms = model.Bedrooms,
                    Bathrooms = model.Bathrooms,
                    Space = model.Space,
                    Price = model.Price,        
                    Date = DateTime.UtcNow,
                    IsSold=false,
                    Purpose=model.Purpose
                };

                // 2. Add the property to the database
                _Context.Properties.Add(newProperty);
                await _Context.SaveChangesAsync();

                // 3. Prepare folder for images using the property ID
                var propertyFolderPath = Path.Combine("wwwroot", "Properties", newProperty.Id.ToString());
                if (!Directory.Exists(propertyFolderPath))
                {
                    Directory.CreateDirectory(propertyFolderPath);
                }

                // 4. Save main image
                if (model.MainImage != null && model.MainImage.Length > 0)
                {
                    var mainImageExtension = Path.GetExtension(model.MainImage.FileName);
                    var mainImageFileName = $"main-image{mainImageExtension}";
                    var mainImagePath = Path.Combine(propertyFolderPath, mainImageFileName);
                    newProperty.MainImagePath = $"/Properties/{newProperty.Id}/{mainImageFileName}";

                    using (var stream = new FileStream(mainImagePath, FileMode.Create))
                    {
                        await model.MainImage.CopyToAsync(stream);
                    }
                }

                // 5. Save additional images
                if (model.AdditionalImages != null && model.AdditionalImages.Any())
                {
                    for (int i = 0; i < model.AdditionalImages.Count; i++)
                    {
                        var additionalImage = model.AdditionalImages[i];
                        var additionalImageExtension = Path.GetExtension(additionalImage.FileName);
                        var imagePath = Path.Combine(propertyFolderPath, $"image{i + 1}{additionalImageExtension}");

                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await additionalImage.CopyToAsync(fileStream);
                        }

                        var propertyImage = new PropertyImage
                        {
                            PropertyId = newProperty.Id,
                            ImagePath = $"/Properties/{newProperty.Id}/image{i + 1}{additionalImageExtension}"
                        };

                        _Context.PropertyImages.Add(propertyImage);
                    }
                }

                // 6. Link property to user
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userProperty = new UserProperty
                    {
                        PropertyId = newProperty.Id,
                        UserId = userId,
                        Type = RelationshipType.Created | RelationshipType.Viewed
                    };

                    _Context.UserProperties.Add(userProperty);
                await SendPropertyNotificationAsync(newProperty);
                }

                // Save all changes to the database
                await _Context.SaveChangesAsync();
                return newProperty.Id;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory error: {ex.Message}");
                throw new Exception("Failed to create directory for property images.");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File error: {ex.Message}");
                throw new Exception("An error occurred while saving property images.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw new Exception("Database error while saving property information.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                throw new Exception("An unexpected error occurred while adding the property.");
            }
        }


        public async Task<bool> UpdateProperty(int propertyId, UpdatePropertyModel model)
        {
            try
            {
                // 1. Find the property to update
                var existingProperty = await _Context.Properties
                    .Include(p => p.PropertyImages) // Include related images if any
                    .FirstOrDefaultAsync(p => p.Id == propertyId);

                if (existingProperty == null)
                {
                    throw new Exception("Property not found.");
                }

                // 2. Update the property fields
                existingProperty.Title = model.Title;
                existingProperty.Location = model.Location;
                existingProperty.Description = model.Description;
                existingProperty.Type = model.Type;
                existingProperty.Bedrooms = model.Bedrooms;
                existingProperty.Bathrooms = model.Bathrooms;
                existingProperty.Space = model.Space;
                existingProperty.Price = model.Price;
                existingProperty.Purpose = model.Purpose;

                // 3. Handle main image update (if any)
                if (model.MainImage != null && model.MainImage.Length > 0)
                {
                    // Remove the old main image file if it exists
                    if (!string.IsNullOrEmpty(existingProperty.MainImagePath))
                    {
                        var oldImagePath = Path.Combine("wwwroot", existingProperty.MainImagePath.TrimStart('/'));
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }

                    // Save new main image
                    var propertyFolderPath = Path.Combine("wwwroot", "Properties", propertyId.ToString());
                    if (!Directory.Exists(propertyFolderPath))
                    {
                        Directory.CreateDirectory(propertyFolderPath);
                    }

                    var mainImageExtension = Path.GetExtension(model.MainImage.FileName);
                    var mainImageFileName = $"main-image{mainImageExtension}";
                    var mainImagePath = Path.Combine(propertyFolderPath, mainImageFileName);
                    existingProperty.MainImagePath = $"/Properties/{propertyId}/{mainImageFileName}";

                    using (var stream = new FileStream(mainImagePath, FileMode.Create))
                    {
                        await model.MainImage.CopyToAsync(stream);
                    }
                }
                foreach (var existingImage in existingProperty.PropertyImages)
                {
                    // Check if the image is not in the AdditionalImagesURL list
                    if (!model.AdditionalImagesURL.Contains(existingImage.ImagePath))
                    {
                        // Delete the image file from the server if it exists
                        var oldImagePath = Path.Combine("wwwroot", existingImage.ImagePath.TrimStart('/'));
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }

                        // Remove the image entry from the database
                        _Context.PropertyImages.Remove(existingImage);
                    }
                }
                // 4. Handle additional images update (if any)
                if (model.AdditionalImages != null && model.AdditionalImages.Any())
                {
                    // Remove existing property images first



                    // Save new additional images
                    var propertyFolderPath = Path.Combine("wwwroot", "Properties", propertyId.ToString());
                    for (int i = 0; i < model.AdditionalImages.Count; i++)
                    {
                        var additionalImage = model.AdditionalImages[i];
                        var additionalImageExtension = Path.GetExtension(additionalImage.FileName);
                        var imagePath = Path.Combine(propertyFolderPath, $"image{i + 1}{additionalImageExtension}");
                        var relativeImagePath = $"/Properties/{propertyId}/image{i + 1}{additionalImageExtension}";
                        int count= 0;
                        while (model.AdditionalImagesURL.Contains(relativeImagePath))
                        {
                             imagePath = Path.Combine(propertyFolderPath, $"image{count + 1}{additionalImageExtension}");
                            relativeImagePath = $"/Properties/{propertyId}/image{count + 1}{additionalImageExtension}";

                            count++;

                        }
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await additionalImage.CopyToAsync(fileStream);
                        }

                        var propertyImage = new PropertyImage
                        {
                            PropertyId = propertyId,
                            ImagePath = $"/Properties/{propertyId}/image{i + 1}{additionalImageExtension}"
                        };

                        _Context.PropertyImages.Add(propertyImage);
                    }
                }

                // 5. Update the property-to-user relationship if necessary
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userProperty = await _Context.UserProperties
                        .FirstOrDefaultAsync(up => up.PropertyId == propertyId && up.UserId == userId);

                    if (userProperty == null)
                    {
                        // If no existing relationship, add a new one
                        userProperty = new UserProperty
                        {
                            PropertyId = propertyId,
                            UserId = userId,
                            Type = RelationshipType.Created | RelationshipType.Viewed
                        };

                        _Context.UserProperties.Add(userProperty);
                    }
                }

                // 6. Save the updated data to the database
                await _Context.SaveChangesAsync();
                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory error: {ex.Message}");
                throw new Exception("Failed to create directory for property images.");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File error: {ex.Message}");
                throw new Exception("An error occurred while saving property images.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw new Exception("Database error while updating property information.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                throw new Exception("An unexpected error occurred while updating the property.");
            }
        }

        public async Task<List<Property>> GetSavedProperties(string userId)
            {
 

                // Query the database for properties saved by the user
                var savedProperties = await _Context.UserProperties
                    .Where(up => up.UserId == userId && up.Type.HasFlag(RelationshipType.Saved))
                    .Select(up => up.Property)
                    .ToListAsync();

                return savedProperties;
            }
        public async Task SendPropertyNotificationAsync(Property property)
        {
            // Get all users with the "User" role (excluding Admin and SuperAdmin)
            var allUsers = new List<IdentityUser>();
            var userRoleExists = await _roleManager.RoleExistsAsync("User");
            var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            var superAdminRoleExists = await _roleManager.RoleExistsAsync("SuperAdmin");

            if (userRoleExists || adminRoleExists || superAdminRoleExists)
            {
                // Get users in the User role
                if (userRoleExists)
                {
                    allUsers.AddRange(await _userManager.GetUsersInRoleAsync("User"));
                }

                // Get users in the Admin role
                if (adminRoleExists)
                {
                    allUsers.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));
                }

                // Get users in the SuperAdmin role
                if (superAdminRoleExists)
                {
                    allUsers.AddRange(await _userManager.GetUsersInRoleAsync("SuperAdmin"));
                }

                foreach (var user in allUsers)
                {
                    if (property.Location == (_Context.Users.Where(u => u.Id == user.Id).FirstOrDefault()).Location){
                        // Create the notification message
                        string notificationMessage = $"A new property has been added that may be suitable for your location preferences. " +
                                                     $"Property Title: {property.Title}, Location: {property.Location}. " +
                                                     $" <a href=\"/HomePage/PropertyDetails/{property.Id}\">click here to view</a>";

                        // Send the notification to the user

                        await SendNotificationToUser(user, notificationMessage);
                        await _userService.SendEmail("test", user.UserName, "A new property has been added that may be suitable for your", notificationMessage);

                    }
                }
            }
        }


        public async Task<bool> RemoveProperty(int propertyId)
        {
            try
            {
                // Fetch the property from the database
                var property = await _Context.Properties
                    .Include(p => p.Appointments)
                    .Include(p => p.PropertyImages)
                    .FirstOrDefaultAsync(p => p.Id == propertyId);

                if (property == null)
                {
                    throw new Exception("Property not found.");
                }

                // Check if the property has any appointments
                if ((property.Appointments != null && property.Appointments.Any())||property.IsSold)
                {
                    return false;
                }

                // Remove property images
                var propertyFolderPath = Path.Combine("wwwroot", "Properties", propertyId.ToString());
                if (Directory.Exists(propertyFolderPath))
                {
                    Directory.Delete(propertyFolderPath, true);
                }

                // Remove additional images from the database
                if (property.PropertyImages != null && property.PropertyImages.Any())
                {
                    _Context.PropertyImages.RemoveRange(property.PropertyImages);
                }

                // Remove relationships with users
                var userProperties = _Context.UserProperties.Where(up => up.PropertyId == propertyId);
                _Context.UserProperties.RemoveRange(userProperties);

                // Remove the property itself
                _Context.Properties.Remove(property);

                // Save changes to the database
                await _Context.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                throw new Exception("An unexpected error occurred while removing the property.");
            }
        }

        public async Task SendNotificationToUser(IdentityUser user, string description)
        {
            // This is a placeholder for sending notifications to the user
            // Depending on your notification system, you might send an email, push notification, or another type of notification

            var notification = new Notification
            {
                UserId = user.Id,
                Content = description,
                CreatedAt = DateTime.Now
            };

            _Context.Notifications.Add(notification);
            await _Context.SaveChangesAsync();

            // Optionally, you can send an email or other notification method to the user
        }

        public async Task<bool> RemoveSavedProperty(string userId, int propertyId)
        {
            // Find the saved property entry for the user and property


            return true;
        }
        public async Task<bool> RemoveAllSavedProperties(string userId)
        {
            // Find the saved property entry for the user and property


            return true;
        }



        public  async Task<List<Tables.Property>> GetAllProperties(List<Tables.Property> allProperties, string location = null, string purpose = null, double minPrice = 40000, double maxPrice = 1000000,
    double minSpace = 40, double maxSpace = 3000, int bedrooms = 0, int bathrooms = 0, string type = "Select Property Type", string sort = "Sort by"
    )
        {




            if (!string.IsNullOrEmpty(location))
            {
                // Split the input location string into individual location keywords
                var locationKeywords = location.Split(',')
                                               .Select(l => l.Trim().ToLower())  // Remove spaces and make lowercase for comparison
                                               .Where(l => !string.IsNullOrEmpty(l))  // Remove any empty entries
                                               .ToList();

                // Filter the properties by matching location keywords (case insensitive, partial match)
                allProperties = allProperties.Where(p =>
                    locationKeywords.Any(keyword => p.Location.ToLower().Contains(keyword)))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(purpose) && purpose != "All")
            {
                allProperties = allProperties.Where(p => p.Purpose == purpose).ToList();
            }
            // Filter by price range
            if (minPrice > 40000)
            {
                allProperties = allProperties.Where(p => p.Price >= (decimal)minPrice).ToList();
            }

            if (maxPrice != 1000000)
            {
                allProperties = allProperties.Where(p => p.Price <= (decimal)maxPrice).ToList();
            }


            // Filter by space (Area)
            if (minSpace > 40)
            {
                allProperties = allProperties
                    .Where(p => p.Space >= (decimal)minSpace)
                    .ToList();
            }
            if (maxSpace < 3000)
            {
                allProperties = allProperties
                    .Where(p => p.Space <= (decimal)maxSpace)
                    .ToList();
            }

            // Filter by bedrooms
            if (bedrooms > 0)
            {
                allProperties = allProperties
                    .Where(p => p.Bedrooms >= bedrooms)
                    .ToList();
            }

            // Filter by bathrooms
            if (bathrooms > 0)
            {
                allProperties = allProperties.Where(p => p.Bathrooms >= bathrooms).ToList();
            }

            // Filter by type
            if (type != "Select Property Type")
            {
                allProperties = allProperties.Where(p => p.Type.ToLower()==type.ToLower()).ToList();
            }

            // Sorting logic using orderCase values
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "recent":
                        // Assuming we have a 'DatePosted' or similar property to sort by
                        allProperties = allProperties.OrderByDescending(p => p.Id).ToList();
                        break;
                    case "min price":
                        allProperties = allProperties.OrderBy(p => p.Price).ToList();
                        break;
                    case "max price":
                        allProperties = allProperties.OrderByDescending(p => p.Price).ToList();
                        break;
                    case "oldest":
                        // Assuming we have a 'DatePosted' or similar property to sort by
                        allProperties = allProperties.OrderBy(p => p.Id).ToList();
                        break;
                    default:
                        // Default sorting can be based on price or other criteria if needed
                        allProperties = allProperties.OrderBy(p => p.Price).ToList();
                        break;
                }
            }

            return allProperties;
        }


        public async Task<List<TimeSpan>> GetAvailableTimes(int propertyId, DateTime date)
        {
            // All available times from 8 AM to 9 PM
            var availableTimes = Enumerable.Range(8, 14).Select(hour => TimeSpan.FromHours(hour)).ToList();

            // Get all booked times for the property on the given date
            var bookedTimes = await _Context.Appointments
                .Where(a => a.PropertyId == propertyId && a.AppointmentDate.Date == date.Date)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            // Remove booked times from available times
            availableTimes = availableTimes.Except(bookedTimes).ToList();

            return availableTimes;
        }

        public async Task<bool> BookAppointment(string userId, int propertyId, DateTime date, TimeSpan time, string appointmentType)
        {
            // Check if the time slot is already booked
            var isBooked = await _Context.Appointments.AnyAsync(a =>
                a.PropertyId == propertyId &&
                a.AppointmentDate.Date == date.Date &&
                a.AppointmentTime == time);

            if (isBooked)
                return false;

            // Create a new appointment
            var appointment = new Appointment
            {
                UserId = userId,
                PropertyId = propertyId,
                AppointmentDate = date,
                AppointmentTime = time,
                AppointmentType = appointmentType,
                IsConfirmed = false
            };

            await _Context.Appointments.AddAsync(appointment);
            await _Context.SaveChangesAsync();
            return true;
        }
    }
}
