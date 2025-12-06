using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Worknest.Services.Identity.Models;

namespace Worknest.Services.Identity.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var resetLink = $"{_emailSettings.FrontendBaseUrl}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = "Reset Your Password - Worknest";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center;'>
                            <h1 style='color: white; margin: 0;'>Worknest</h1>
                        </div>
                        <div style='padding: 40px 20px; background: #f9fafb;'>
                            <h2 style='color: #1f2937;'>Reset Your Password</h2>
                            <p style='color: #4b5563; line-height: 1.6;'>
                                You've requested to reset your password. Click the button below to create a new password.
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' 
                                   style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                          color: white; 
                                          padding: 14px 28px; 
                                          text-decoration: none; 
                                          border-radius: 8px;
                                          font-weight: bold;
                                          display: inline-block;'>
                                    Reset Password
                                </a>
                            </div>
                            <p style='color: #6b7280; font-size: 14px;'>
                                If you didn't request this, you can safely ignore this email.
                            </p>
                            <p style='color: #6b7280; font-size: 14px;'>
                                This link will expire in 24 hours.
                            </p>
                        </div>
                        <div style='padding: 20px; text-align: center; background: #1f2937;'>
                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>
                                © 2024 Worknest. All rights reserved.
                            </p>
                        </div>
                    </body>
                    </html>",
                TextBody = $"Reset your password by visiting: {resetLink}"
            };

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTlsWhenAvailable);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                throw;
            }
        }
    }
}
