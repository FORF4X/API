using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, UserManager<User> userManager, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest("Email is already in use.");

            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PrivateNumber = registerDto.PrivateNumber,
                ActivationCode = registerDto.ActivationCode
                
            };

            if (registerDto.Photo != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await registerDto.Photo.CopyToAsync(memoryStream);
                    user.Photo = memoryStream.ToArray();
                }
            }

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var response = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PrivateNumber,
                Photo = user.Photo != null ? Convert.ToBase64String(user.Photo) : null
            };

            return Ok(response);
        }

        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctor([FromForm] RegisterDoctorDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest("Email is already in use.");

            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PrivateNumber = registerDto.PrivateNumber,
                ActivationCode = registerDto.ActivationCode
            };

            if (registerDto.Photo != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await registerDto.Photo.CopyToAsync(memoryStream);
                    user.Photo = memoryStream.ToArray();
                }
            }

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var doctorProfile = new Doctor
            {
                UserId = user.Id,
                Category = registerDto.Category
            };

            if (registerDto.CV != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await registerDto.CV.CopyToAsync(memoryStream);
                    doctorProfile.CV = memoryStream.ToArray();
                }
            }
            
            await _context.Doctors.AddAsync(doctorProfile);
            await _context.SaveChangesAsync();

            var response = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PrivateNumber,
                Photo = user.Photo != null ? Convert.ToBase64String(user.Photo) : null,
                DoctorDetails = new
                {
                    doctorProfile.Category,
                    CV = doctorProfile.CV != null ? Convert.ToBase64String(doctorProfile.CV) : null
                }
            };

            return Ok(response);
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.DoctorProfile)
                .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null) return Unauthorized("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized("Invalid email or password.");

            var token = await _tokenService.GenerateToken(user);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            var profile = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PrivateNumber,
                Photo = user.Photo != null ? Convert.ToBase64String(user.Photo) : null,
                Role = role,
                DoctorDetails = role == "Doctor" ? new
                {
                    user.DoctorProfile.Category,
                    CV = user.DoctorProfile.CV != null ? Convert.ToBase64String(user.DoctorProfile.CV) : null
                } : null
            };

            return Ok(new { token, profile });
        }

        [HttpGet("get-doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User) 
                .ToListAsync();

            var doctorProfiles = doctors.Select(d => new
            {
                d.User.FirstName,
                d.User.LastName,
                d.User.Email,
                d.User.PrivateNumber,
                d.Category,
                Photo = d.User.Photo != null ? Convert.ToBase64String(d.User.Photo) : null,
                CV = d.CV != null ? Convert.ToBase64String(d.CV) : null
            });

            return Ok(doctorProfiles);
        }
        
        [HttpPost("book-visit")]
        public async Task<IActionResult> BookVisit([FromBody] BookingDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User is not authenticated.");

            var doctor = await _context.Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == bookingDto.DoctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.DoctorId == bookingDto.DoctorId && b.AppointmentDateTime == bookingDto.AppointmentDateTime);
    
            if (existingBooking)
            {
                return BadRequest("The selected time slot is already booked.");
            }
            
            var booking = new Booking
            {
                UserId = userId,
                DoctorId = bookingDto.DoctorId,
                AppointmentDateTime = bookingDto.AppointmentDateTime,
                Description = bookingDto.Description
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking successful.", bookingId = booking.Id });
        }

        [HttpGet("available-slots/{doctorId}")]
        public async Task<IActionResult> GetAvailableSlots(string doctorId, [FromQuery] DateTime date)
        {
            var doctor = await _context.Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            var availableSlots = new List<DateTime>();
            var startOfDay = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0);
            
            for (int i = 0; i < 8; i++)  
            {
                availableSlots.Add(startOfDay.AddHours(i));
            }

            var bookedSlots = await _context.Bookings
                .Where(b => b.DoctorId == doctorId && b.AppointmentDateTime.Date == date.Date)
                .Select(b => b.AppointmentDateTime)
                .ToListAsync();

            var freeSlots = availableSlots.Where(slot => !bookedSlots.Contains(slot)).ToList();

            return Ok(freeSlots);
        }
        
        [HttpPut("edit-appointment/{appointmentId}")]
        public async Task<IActionResult> EditAppointment(int appointmentId, [FromBody] EditAppointmentDto editDto)
        {
            var appointment = await _context.Bookings.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound("Appointment not found.");

            if (appointment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized("You can only edit your own appointments.");

            var conflictingBooking = await _context.Bookings
                .AnyAsync(b => b.DoctorId == appointment.DoctorId && 
                               b.AppointmentDateTime == editDto.NewAppointmentDateTime && 
                               b.Id != appointmentId);

            if (conflictingBooking)
                return BadRequest("The selected time slot is already booked. Please choose another time.");

            appointment.AppointmentDateTime = editDto.NewAppointmentDateTime;
            appointment.Description = editDto.NewDescription ?? appointment.Description;

            _context.Bookings.Update(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment updated successfully.", appointmentId = appointment.Id });
        }
        
        [HttpDelete("delete-appointment/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)
        {
            var appointment = await _context.Bookings.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound("Appointment not found.");

            if (appointment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized("You can only delete your own appointments.");

            _context.Bookings.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment deleted successfully." });
        }
    }
}
