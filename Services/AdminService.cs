using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Real_State.Data;
using Real_State.Models;
using System.Security.Claims;
using Real_State.Tables;
using Microsoft.EntityFrameworkCore;
using Real_State.Controllers;
using System.Text;

namespace Real_State.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _Context;

        public AdminService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _Context = context;
            _userManager = userManager;
        }
        public async Task<bool> UpdateUserProfileAsync(
              HttpContext httpContext,
              string userId,
              ProfileModel model)
        {
            // Fetch user from database
            var user = await _Context.Users.FindAsync(userId);
            if (user == null) return false;

            // Handle photo upload
            if (model.Photo != null && model.Photo.Length > 0)
            {
                string folderPath = Path.Combine("wwwroot", "Users Images", user.Email);

                if (Directory.Exists(folderPath))
                {
                    foreach (var file in Directory.GetFiles(folderPath))
                    {
                        System.IO.File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(folderPath);
                }

                string newFileName = $"{Path.GetFileNameWithoutExtension(model.Photo.FileName)}{Path.GetExtension(model.Photo.FileName)}";
                string filePath = Path.Combine(folderPath, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                user.image = filePath; // Update image path in the database
            }

            // Update user properties
            user.Name = model.Name;
            user.Location = model.Location;
            _Context.Users.Update(user);
            await _Context.SaveChangesAsync();

            // Update claims
            await UpdateUserClaimsAsync(httpContext, user);

            return true;
        }

        private async Task UpdateUserClaimsAsync(HttpContext httpContext, User user)
        {
            // Sign out existing user
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("ImgUrl", user.image ?? string.Empty)
        };

            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null && roles.Count > 0)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );
        }

        public async Task<(bool success, string message)> TogglePropertySaveStatusAsync(string userId, int propertyId)
        {
            // Check if the user already has a relationship with this property
            var existingUserProperty = await _Context.UserProperties
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PropertyId == propertyId);

            if (existingUserProperty != null)
            {
                if (existingUserProperty.Type.HasFlag(RelationshipType.Saved))
                {
                    // If the property is saved, unsave it
                    existingUserProperty.Type &= ~RelationshipType.Saved; // Remove "Saved" flag
                    _Context.UserProperties.Update(existingUserProperty);
                    await _Context.SaveChangesAsync();

                    return (true, "Property unsaved successfully!");
                }
                else
                {
                    // If it's not saved, save it
                    existingUserProperty.Type |= RelationshipType.Saved; // Add "Saved" flag
                    _Context.UserProperties.Update(existingUserProperty);
                    await _Context.SaveChangesAsync();

                    return (true, "Property saved successfully!");
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

                return (true, "Property saved successfully!");
            }
        }


        // Recursive method to get properties saved status
        public async Task<List<PropertiesSavedStatus>> GetPropertiesSavedStatusRecursive(
            List<Property> properties, string userId, int index)
        {
            if (index >= properties.Count)
            {
                return new List<PropertiesSavedStatus>();
            }

            var property = properties[index];
            var existingUserProperty = await _Context.UserProperties
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PropertyId == property.Id);

            var restSavedStatus = await GetPropertiesSavedStatusRecursive(properties, userId, index + 1);

            var status = existingUserProperty != null && existingUserProperty.Type.HasFlag(RelationshipType.Saved);
            restSavedStatus.Insert(0, new PropertiesSavedStatus { Id = property.Id, Status = status });

            return restSavedStatus;
        }

        // Recursive method to get agents for the properties
        public async Task<List<AgentModel>> GetAgentsRecursive(List<Property> properties, int index)
        {
            if (index >= properties.Count)
            {
                return new List<AgentModel>();
            }

            var property = properties[index];
            var agentId = (await _Context.UserProperties
                 .Where(p => p.PropertyId == property.Id &&
                ((p.Type & RelationshipType.Created) == RelationshipType.Created ||
                 (p.Type & RelationshipType.Viewed) == RelationshipType.Viewed))
                    .FirstOrDefaultAsync())?.UserId;

            var agent = agentId != null ? await _Context.Users.Where(u => u.Id == agentId).FirstOrDefaultAsync() : null;

            var restAgents = await GetAgentsRecursive(properties, index + 1);

            if (agent != null)
            {
                restAgents.Insert(0, new AgentModel
                {
                    Id = agent.Id,
                    Name = agent.Name,
                    Email = agent.Email,
                    Image = agent.image,
                    Phone = agent.PhoneNumber
                });
            }

            return restAgents;
        }


        #region Helper Methods

        // Function to read locations from the text file
        public List<string> ReadLocationsFromFile(string filePath)
        {
            var locations = new List<string>();

            // Check if file exists
            if (System.IO.File.Exists(filePath))
            {
                // Read all lines from the file
                string[] lines = System.IO.File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            locations.Add(DecodeBinaryString(line)); // Decode from binary
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle invalid binary string
                        Console.WriteLine($"Error decoding binary line: {line}, Exception: {ex.Message}");
                    }
                }
            }

            return locations;
        }
        public bool IsLocationUsedByAnyUser(string location)
        {
            return _Context.Users.Any(user => user.Location == location);

        }

        // Function to save locations to the text file
        public string SaveLocationsToFile(List<string> locations, string filePath)
        {
            var oldList = ReadLocationsFromFile(filePath);
            var removedLocations = oldList.Except(locations).ToList();
            var addedLocations = locations.Except(oldList).ToList();

            // Check for removed locations
            foreach (var removedLocation in removedLocations)
            {
                if (IsLocationUsedByAnyUser(removedLocation))
                {
                    return $"Cannot remove location '{removedLocation}' as it is associated with users.";
                }

                // Remove the location from the old list
                oldList.Remove(removedLocation);
            }

            // Check for updated locations
            var updateMessages = new List<string>();
            foreach (var location in locations)
            {
                var similarLocation = oldList.FirstOrDefault(oldLocation =>
                    IsSimilarLocation(oldLocation, location));

                if (similarLocation != null && similarLocation != location)
                {
                    UpdateUserLocations(similarLocation, location);
                    updateMessages.Add($"Updated location '{similarLocation}' to '{location}'.");

                    oldList.Remove(similarLocation);
                    oldList.Add(location);
                }
            }

            // Add new locations if any
            if (addedLocations.Any())
            {
                foreach (var newLocation in addedLocations)
                {
                    oldList.Add(newLocation); // Add new location to the old list
                }
            }

            // Encode each location to binary and write to the file
            var binaryLines = oldList.Select(EncodeToBinary).ToList();
            System.IO.File.WriteAllLines(filePath, binaryLines);

            // Return success message with updates or confirmation
            var resultMessages = new List<string>();

            if (updateMessages.Any())
            {
                resultMessages.AddRange(updateMessages);
            }
            if (addedLocations.Any())
            {
                resultMessages.Add($"Added new location(s): {string.Join(", ", addedLocations)}.");
            }

            if (resultMessages.Any())
            {
                return string.Join(" ", resultMessages);
            }

            return "Locations saved successfully without changes.";
        }

        public bool IsSimilarLocation(string oldLocation, string newLocation)
        {
            // Normalize both locations by replacing spaces with hyphens and vice versa
            string normalizedOldLocation = oldLocation.Replace(" ", "-").Replace("-", " ");
            string normalizedNewLocation = newLocation.Replace(" ", "-").Replace("-", " ");

            // Perform case-insensitive comparison after normalization
            return string.Equals(normalizedOldLocation, normalizedNewLocation, StringComparison.OrdinalIgnoreCase);
        }
        public void UpdateUserLocations(string oldLocation, string newLocation)
        {

            var usersToUpdate = _Context.Users.Where(user => user.Location == oldLocation).ToList();

            foreach (var user in usersToUpdate)
            {
                user.Location = newLocation;
            }
            _Context.Users.UpdateRange(usersToUpdate);
            _Context.SaveChanges();

        }

        // Function to encode a string into binary
        private string EncodeToBinary(string input)
        {
            return string.Join(string.Empty, input.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')));
        }

        // Function to decode binary strings back into text
        private string DecodeBinaryString(string binary)
        {
            var text = new StringBuilder();

            for (int i = 0; i < binary.Length; i += 8)
            {
                string byteString = binary.Substring(i, 8);
                char character = (char)Convert.ToByte(byteString, 2);
                text.Append(character);
            }

            return text.ToString();
        }

        #endregion


    }
}
