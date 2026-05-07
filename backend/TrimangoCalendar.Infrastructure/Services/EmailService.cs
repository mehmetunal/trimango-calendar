public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // SMTP ayarlarını config'den al
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        var smtpUser = _configuration["Email:Username"];
        var smtpPass = _configuration["Email:Password"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"];
        
        using var client = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPass)
        };
        
        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
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
    
    public async Task SendBulkEmailAsync(List<string> to, string subject, string body)
    {
        var tasks = to.Select(recipient => SendEmailAsync(recipient, subject, body));
        await Task.WhenAll(tasks);
    }
}

