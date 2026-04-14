using System.ComponentModel.DataAnnotations;

namespace Worknest.Services.Identity.Models
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }
}