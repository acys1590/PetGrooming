using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace PetGroomingSystem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _sender;
        private readonly string _password;
        private readonly string _host;
        private readonly int _port;

        public EmailSender(IConfiguration config)
        {
            _sender = config.GetValue<string>("SMTP:Sender")!;
            _password = config.GetValue<string>("SMTP:SecretKey")!;
            _host = config.GetValue<string>("SMTP:Host")!;
            _port = config.GetValue<int>("SMTP:Port");
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_sender, "Pet Grooming Support"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(email));

            using var smtpClient = new SmtpClient(_host, _port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_sender, _password)
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
