namespace AIDevAPI.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendPasswordResetEmailAsync(string to, string userName, string resetLink);
    Task SendWelcomeEmailAsync(string to, string userName);
    Task SendEmailConfirmationAsync(string to, string userName, string confirmationLink);
}
