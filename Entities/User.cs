using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class User : IdentityUser
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string ActivationCode { get; set; }

    [Required]
    public string PrivateNumber { get; set; }
    
    public DateTime ActivationCodeExpiration { get; set; }
    
    public byte[]? Photo { get; set; }
    
    public Doctor DoctorProfile { get; set; }
}
