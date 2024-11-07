using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Services;
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
                .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null) return Unauthorized("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized("Invalid email or password.");

            var token = await _tokenService.GenerateToken(user);

            return Ok(new { token });
        }
    }
}