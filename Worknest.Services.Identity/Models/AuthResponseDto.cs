namespace Worknest.Services.Identity.Models
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? JobTitle { get; set; }
    }
}