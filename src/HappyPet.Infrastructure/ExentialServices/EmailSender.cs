using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Mail;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace HappyPet.Infrastructure.ExentialServices
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPassword = _config["Email:SmtpPassword"];

            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new EmailDeliveryException("SMTP კონფიგურაცია არ არის შევსებული.");
            }

            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.TryParse(_config["Email:SmtpPort"], out var configuredPort)
                ? configuredPort
                : 587;
            var enableSsl = !bool.TryParse(_config["Email:EnableSsl"], out var configuredSsl) || configuredSsl;
            var from = _config["Email:From"] ?? smtpUser;

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(to);

            try
            {
                await smtpClient.SendMailAsync(message, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EmailDeliveryException("ელ-ფოსტის გაგზავნა ვერ მოხერხდა.", ex);
            }
        }
    }
}
