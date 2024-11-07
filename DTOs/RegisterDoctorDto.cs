namespace API.DTOs
{
    public class RegisterDoctorDto : RegisterUserDto
    {
        public string Category { get; set; }
        public byte[]? Photo { get; set; } // Adjust as necessary, consider using a file upload approach instead
        public byte[]? CV { get; set; } // Adjust as necessary, consider using a file upload approach instead
    }
}