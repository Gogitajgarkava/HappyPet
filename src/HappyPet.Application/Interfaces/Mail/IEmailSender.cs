namespace HappyPet.Application.Interfaces.Mail
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    }
}
