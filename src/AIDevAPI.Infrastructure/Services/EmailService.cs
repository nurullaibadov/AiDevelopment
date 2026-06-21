using System.Net;
using System.Net.Mail;
using AIDevAPI.Application.Interfaces.Services;
using AIDevAPI.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIDevAPI.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email göndərildi: {To}, Mövzu: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email göndərilmədi: {To}", to);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string to, string userName, string resetLink)
    {
        var subject = "Şifrə Bərpası";
        var body = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
                <h2 style='color:#2563eb;'>Salam, {userName}!</h2>
                <p>Hesabınız üçün şifrə bərpası tələbi aldıq. Yeni şifrə təyin etmək üçün aşağıdakı düyməyə klikləyin:</p>
                <p style='text-align:center;margin:30px 0;'>
                    <a href='{resetLink}' style='background:#2563eb;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;'>Şifrəni Bərpa Et</a>
                </p>
                <p>Əgər bu tələbi siz etməmisinizsə, bu emaili nəzərə almaya bilərsiniz.</p>
                <p>Link 1 saat ərzində etibarlıdır.</p>
                <hr/>
                <p style='color:#888;font-size:12px;'>AI Development Platform</p>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        var subject = "Platformamıza xoş gəlmisiniz!";
        var body = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
                <h2 style='color:#16a34a;'>Xoş gəlmisiniz, {userName}!</h2>
                <p>Qeydiyyatınız uğurla tamamlandı. İndi platformamızın bütün imkanlarından istifadə edə bilərsiniz.</p>
                <hr/>
                <p style='color:#888;font-size:12px;'>AI Development Platform</p>
            </div>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailConfirmationAsync(string to, string userName, string confirmationLink)
    {
        var subject = "Email Təsdiqi";
        var body = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;'>
                <h2 style='color:#2563eb;'>Salam, {userName}!</h2>
                <p>Qeydiyyatı tamamlamaq üçün emailinizi təsdiqləyin:</p>
                <p style='text-align:center;margin:30px 0;'>
                    <a href='{confirmationLink}' style='background:#2563eb;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;'>Emaili Təsdiqlə</a>
                </p>
                <hr/>
                <p style='color:#888;font-size:12px;'>AI Development Platform</p>
            </div>";

        await SendEmailAsync(to, subject, body);
    }
}
