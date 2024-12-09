using System;

namespace API.DTOs
{
    public class BookingDto
    {
        public string DoctorId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Description { get; set; }
    }
}