using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(5, ErrorMessage = "First name must be at least 5 characters long.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Activation code is required.")]
        public string ActivationCode { get; set; }

        [Required(ErrorMessage = "Private number is required.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Private number must be exactly 11 digits.")]
        public string PrivateNumber { get; set; }
        
        public IFormFile? Photo { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[\W_]).{8,}$", ErrorMessage = "Password must be at least 8 characters long, containing at least one symbol and one number.")]
        public string Password { get; set; }
    }
}