using MailKit.Net.Smtp;
using MimeKit;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace Roomiebill.Server.Common.Validators
{
    public class RegisterVerify
    {
        public static async Task<VerifiyCodeModel> SendVerificationEmail(string recipientEmail)
        {
            var verificationCode = GenerateVerificationCode();
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("RoomieBill register verify", "roomiebillservice@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", recipientEmail));
            emailMessage.Subject = "Verify Your Email";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {verificationCode}"
            };
            
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("roomiebillservice@gmail.com", "lzuy clck kgct auwn");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

            return new VerifiyCodeModel { VerifyCode = verificationCode };
        }

        public static string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates a 6-digit code
        }
    }
}