using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<User> userManager, DataContext context, ITokenService tokenService)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser(RegisterUserDto userDto)
        {
            var user = new User
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                ActivationCode = userDto.ActivationCode,
                PrivateNumber = userDto.PrivateNumber,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");

            var loginDto = new LoginDto
            {
                Email = userDto.Email,
                Password = userDto.Password
            };

            var loginResult = await Login(loginDto);

            return loginResult;
        }

        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto doctorDto)
        {
            var user = new User
            {
                UserName = doctorDto.Email,
                Email = doctorDto.Email,
                FirstName = doctorDto.FirstName,
                LastName = doctorDto.LastName,
                ActivationCode = doctorDto.ActivationCode,
                PrivateNumber = doctorDto.PrivateNumber,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, doctorDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var doctor = new Doctor
            {
                UserId = user.Id,
                Category = doctorDto.Category,
                Photo = doctorDto.Photo,
                CV = doctorDto.CV
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, "Doctor");

            var loginDto = new LoginDto
            {
                Email = doctorDto.Email,
                Password = doctorDto.Password
            };

            var loginResult = await Login(loginDto);

            return loginResult;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.DoctorProfile) // Include Doctor profile if it exists
                .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null) return Unauthorized("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized("Invalid email or password.");

            var token = await _tokenService.GenerateToken(user);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            // Prepare profile information based on the role
            var profile = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                Photo = user.Photo != null ? Convert.ToBase64String(user.Photo) : null,
                Role = role,
                DoctorDetails = role == "Doctor" ? new 
                {
                    user.DoctorProfile.Category,
                    Photo = user.DoctorProfile.Photo != null ? Convert.ToBase64String(user.DoctorProfile.Photo) : null,
                    CV = user.DoctorProfile.CV != null ? Convert.ToBase64String(user.DoctorProfile.CV) : null
                } : null
            };

            return Ok(new { token, profile });
        }

        
        [Authorize(Roles = "Doctor")]
        [HttpGet("get-doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors.Include(d => d.User) 
                .ToListAsync();

            var doctorList = doctors.Select(d => new 
            {
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                Photo = d.Photo != null ? Convert.ToBase64String(d.Photo) : null,
                CV = d.CV != null ? Convert.ToBase64String(d.CV) : null
            }).ToList();

            return Ok(doctorList);
        }
        
        [Authorize(Roles = "User")]
        [HttpGet("get-non-doctor-users")]
        public async Task<IActionResult> GetNonDoctorUsers()
        {
            var allUsers = await _userManager.Users.ToListAsync();
    
            var nonDoctorUsers = new List<User>();

            foreach (var user in allUsers)
            {
                if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                {
                    nonDoctorUsers.Add(user);
                }
            }

            var usersDto = nonDoctorUsers.Select(user => new 
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName
            }).ToList();

            return Ok(usersDto);
        }
    }
}