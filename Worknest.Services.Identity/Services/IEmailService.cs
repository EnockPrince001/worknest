namespace Worknest.Services.Identity.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }
}
