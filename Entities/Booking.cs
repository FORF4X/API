using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string DoctorId { get; set; }
        
        [Required]
        public DateTime AppointmentDateTime { get; set; }

        public string Description { get; set; }

        public User User { get; set; }
        public Doctor Doctor { get; set; }
    }
}