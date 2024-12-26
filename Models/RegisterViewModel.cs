using System;

using System.ComponentModel.DataAnnotations;

namespace Real_State.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be more than 2 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Must be a valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#\$^+=!*()@%&]).{8,}$",
                           ErrorMessage = "Password must have at least one uppercase, one lowercase, one number, and one special character.")]
        public string Password { get; set; }
        public string Phone { get; set; }

        public string Message { get; set; }
    }
}