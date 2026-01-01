using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using REGIVA_CR.AB.Services;

namespace REGIVA_CR.LN.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            string? host = _config["EmailSettings:Host"];
            int port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            string? emailFrom = _config["EmailSettings:Email"];
            string? password = _config["EmailSettings:Password"];

            using SmtpClient client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(emailFrom, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(emailFrom!, "REGIVA Soporte"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
    }
}
