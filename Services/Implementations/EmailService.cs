using Hotel_System.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Hotel_System.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendResetPasswordEmailAsync(string toEmail, string toName, string resetLink)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["Email:FromName"],
                _config["Email:Username"]
            ));
            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = "Reset Your Password - Hotel System";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #0f3460; padding: 24px; text-align: center;'>
                        <h2 style='color: white; margin: 0;'>🏨 Hotel System</h2>
                    </div>
                    <div style='padding: 32px; background: #f9f9f9;'>
                        <h3>Hello {toName},</h3>
                        <p>We received a request to reset your password.</p>
                        <p>Click the button below to reset your password. This link will expire in <strong>30 minutes</strong>.</p>
                        <div style='text-align: center; margin: 32px 0;'>
                            <a href='{resetLink}' 
                               style='background: #0f3460; color: white; padding: 14px 28px; 
                                      text-decoration: none; border-radius: 8px; font-size: 16px;'>
                                Reset Password
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>
                            If you did not request this, please ignore this email.
                        </p>
                    </div>
                    <div style='padding: 16px; text-align: center; color: #999; font-size: 12px;'>
                        &copy; 2026 Hotel System. All rights reserved.
                    </div>
                </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"]!),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}