using LibraryAPI.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace LibraryAPI.Utils
{
    public class EmailService: IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration config)
        {
            _configuration = config;
        }

        public void SendMail(Email email)
        {
            var emailObj = new MimeMessage();
            var from = _configuration["EmmailSettings:From"];
            emailObj.From.Add(new MailboxAddress("BMS", from));
            emailObj.To.Add(new MailboxAddress(email.To, email.To));
            emailObj.Subject = email.Subject;
            emailObj.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = string.Format(email.Body)
            };
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_configuration["EmmailSettings:SmtpServer"], 465, true);
                    client.Authenticate(_configuration["EmmailSettings:From"], _configuration["EmmailSettings:Password"]);
                    client.Send(emailObj);
                } catch (Exception ex)
                {
                    throw;
                } finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
