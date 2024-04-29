using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace DocumentManagmentSystem_Demo.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        Task IEmailSender.SendEmailAsync(string email, string subject, string message)
        {
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            var gmailClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            };

            return gmailClient.SendMailAsync(
                               new MailMessage(username, email, subject, message)
                               {
                                   IsBodyHtml = true
                               });
        }
    }
}
