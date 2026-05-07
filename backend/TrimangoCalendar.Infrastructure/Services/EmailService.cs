using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrimangoCalendar.Core.Interfaces;

namespace TrimangoCalendar.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// SendEmailAsync methodunu çalıştırır.
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // SMTP ayarlarını config'den al
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPortStr = _configuration["Email:SmtpPort"];
        var smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;
        var smtpUser = _configuration["Email:Username"];
        var smtpPass = _configuration["Email:Password"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"];

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogError("SMTP configuration is incomplete");
            throw new InvalidOperationException("SMTP configuration is incomplete");
        }

        using var client = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPass)
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName ?? string.Empty),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email gönderildi: {To} - {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email gönderilemedi: {To}", to);
            throw;
        }
    }

    /// <summary>
    /// SendBulkEmailAsync methodunu çalıştırır.
    /// </summary>
    public async Task SendBulkEmailAsync(List<string> to, string subject, string body)
    {
        var tasks = to.Select(recipient => SendEmailAsync(recipient, subject, body));
        await Task.WhenAll(tasks);
    }
}
