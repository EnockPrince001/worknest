namespace Worknest.Services.Identity.Models
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = default!;
        public DateTime Expiration { get; set; }
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string? JobTitle { get; set; }
    }
}