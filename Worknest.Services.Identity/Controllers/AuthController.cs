using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Worknest.Data.Models;
using Worknest.Services.Identity.Models;
using Worknest.Services.Identity.Services;

namespace Worknest.Services.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<User> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        // --- REGISTRATION ENDPOINT ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // 1. Check if user already exists
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { Message = "User already exists with this email." });
            }

            // 2. Create the new user object
            User user = new()
            {
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(), // Required by Identity
                UserName = registerDto.Username
            };

            // 3. Create the user in the database (this hashes the password)
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "User creation failed.", Errors = result.Errors });
            }

            return Ok(new { Message = "User created successfully!" });
        }

        // --- LOGIN ENDPOINT ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Check if user exists
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // 2. Check if password is correct
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // 3. Generate a JWT token
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // This is the User ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authResponse = GenerateJwtToken(authClaims);

            return Ok(new AuthResponseDto
            {
                Token = authResponse.Token,
                Expiration = authResponse.Expiration,
                Email = user.Email,
                Username = user.UserName
            });
        }

        // --- FORGOT PASSWORD ENDPOINT ---
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            // Always return success to prevent email enumeration attacks
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Return success even if user doesn't exist (security best practice)
                return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send email with reset link
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email!, resetToken);
            }
            catch (Exception)
            {
                // Log error internally but don't expose to user
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to send password reset email. Please try again later." });
            }

            return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
        }

        // --- RESET PASSWORD ENDPOINT ---
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid password reset request." });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { Message = "Password reset failed.", Errors = errors });
            }

            return Ok(new { Message = "Password has been reset successfully. You can now login with your new password." });
        }

        // --- TOKEN GENERATION HELPER ---
        private (string Token, DateTime Expiration) GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenValidityInMinutes = Convert.ToDouble(_configuration.GetValue<int>("Jwt:TokenValidityInMinutes", 60)); // Default to 60 mins

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return (
                Token: new JwtSecurityTokenHandler().WriteToken(token),
                Expiration: token.ValidTo
            );
        }
    }
}
