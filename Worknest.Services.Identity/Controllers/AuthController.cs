using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Worknest.Data.Models;
using Worknest.Services.Identity.Models;

namespace Worknest.Services.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
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