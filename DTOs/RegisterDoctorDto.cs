namespace API.DTOs
{
    public class RegisterDoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ActivationCode { get; set; }
        public string PrivateNumber { get; set; }
        public string Category { get; set; }
        public IFormFile? Photo { get; set; }
        public IFormFile? CV { get; set; }
    }
}