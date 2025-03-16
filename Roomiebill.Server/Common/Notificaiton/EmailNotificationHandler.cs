using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Roomiebill.Server.Common.Notification
{
    public class EmailNotificationHandler
    {
        public static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("RoomieBill New Notification", "roomiebillservice@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("roomiebillservice@gmail.com", "lzuy clck kgct auwn");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
