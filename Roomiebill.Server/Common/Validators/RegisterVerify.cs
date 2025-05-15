using MailKit.Net.Smtp;
using MimeKit;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace Roomiebill.Server.Common.Validators
{
    /// <summary>
    /// Handles email verification during user registration process.
    /// </summary>
    public class RegisterVerify
    {
        private const string SENDER_EMAIL = "roomiebillservice@gmail.com";
        private const string SENDER_NAME = "RoomieBill register verify";
        private const string SMTP_SERVER = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        private const string EMAIL_PASSWORD = "lzuy clck kgct auwn";
        private const int CODE_MIN = 100000;
        private const int CODE_MAX = 999999;

        // For testing purposes
        public static IEmailService EmailService { get; set; } = new SmtpEmailService();

        /// <summary>
        /// Sends a verification email containing a 6-digit code to the specified email address.
        /// </summary>
        /// <param name="recipientEmail">The email address to send the verification code to.</param>
        /// <returns>A VerifyCodeModel containing the generated verification code.</returns>
        /// <exception cref="ArgumentNullException">When recipientEmail is null or empty.</exception>
        /// <exception cref="Exception">When email sending fails.</exception>
        public static async Task<VerifiyCodeModel> SendVerificationEmail(string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                throw new ArgumentNullException(nameof(recipientEmail), "Recipient email address cannot be empty.");
            }

            var verificationCode = GenerateVerificationCode();
            var emailMessage = new MimeMessage();
            
            // Format sender with display name and email
            emailMessage.From.Add(new MailboxAddress(SENDER_NAME, SENDER_EMAIL));
            
            // Add recipient email (without display name)
            emailMessage.To.Add(new MailboxAddress(string.Empty, recipientEmail.Trim()));
            
            emailMessage.Subject = "Verify Your Email";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {verificationCode}"
            };
            
            try
            {
                await EmailService.SendEmailAsync(emailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send verification email to {recipientEmail}: {ex.Message}");
            }

            return new VerifiyCodeModel { VerifyCode = verificationCode };
        }

        /// <summary>
        /// Generates a random 6-digit verification code.
        /// </summary>
        /// <returns>A string containing the 6-digit verification code.</returns>
        private static readonly Random random = new Random();
        public static string GenerateVerificationCode()
        {
            return random.Next(CODE_MIN, CODE_MAX).ToString();
        }

        /// <summary>
        /// Validates if a given verification code matches the expected format.
        /// </summary>
        /// <param name="code">The verification code to validate.</param>
        /// <returns>True if the code is valid, false otherwise.</returns>
        public static bool ValidateVerificationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return code.Length == 6 && int.TryParse(code, out int numericCode) && 
                   numericCode >= CODE_MIN && numericCode <= CODE_MAX;
        }
    }

    public interface IEmailService
    {
        Task SendEmailAsync(MimeMessage message);
    }

    public class SmtpEmailService : IEmailService
    {
        public async Task SendEmailAsync(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    "smtp.gmail.com", 
                    587, 
                    MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    "roomiebillservice@gmail.com", 
                    "lzuy clck kgct auwn");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
