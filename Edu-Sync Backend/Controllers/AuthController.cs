using Edu_Sync_Backend.Data;
using Edu_Sync_Backend.DTOs;
using Edu_Sync_Backend.Models;
using Edu_Sync_Backend.Services;
using EduSyncWebAPI.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Edu_Sync_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly PasswordHasher<UserModel> _passwordHasher;

        public AuthController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
            _passwordHasher = new PasswordHasher<UserModel>();
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                // Log the incoming request
                Console.WriteLine($"Registration attempt for email: {dto?.Email}");

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = errors 
                    });
                }

                // Check if email already exists (case insensitive)
                if (await _context.UserModels.AnyAsync(u => 
                    u.Email.ToLower() == dto.Email.ToLower()))
                {
                    Console.WriteLine($"Email already exists: {dto.Email}");
                    return BadRequest(new { 
                        message = "Email already exists",
                        field = "email"
                    });
                }

                // Validate role
                var validRoles = new[] { "Student", "Instructor" };
                if (!validRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Invalid role: {dto.Role}");
                    return BadRequest(new { 
                        message = "Invalid role. Must be 'Student' or 'Instructor'.",
                        field = "role"
                    });
                }


                var user = new UserModel
                {
                    UserId = Guid.NewGuid(),
                    Name = dto.Name.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    Role = dto.Role
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

                _context.UserModels.Add(user);
                await _context.SaveChangesAsync();

                Console.WriteLine($"User registered successfully: {user.Email}");
                return Ok(new { 
                    message = "User registered successfully",
                    userId = user.UserId,
                    email = user.Email,
                    name = user.Name,
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex}");
                return StatusCode(500, new { 
                    message = "An error occurred while processing your request.",
                    error = ex.Message
                });
            }
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _context.UserModels.SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _authService.GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }
    }
}
