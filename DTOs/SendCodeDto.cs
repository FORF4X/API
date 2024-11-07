using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class SendCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}