using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Doctor
{
    [Key]
    [ForeignKey("User")]
    public string UserId { get; set; }
    
    [Required] 
    public string Category { get; set; }

    public byte[]? Photo { get; set; }
    
    public byte[]? CV { get; set; }
    
    public User User { get; set; }
    
}