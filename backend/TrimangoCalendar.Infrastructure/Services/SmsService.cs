using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrimangoCalendar.Core.Interfaces;

namespace TrimangoCalendar.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    
    public SmsService(HttpClient httpClient, IConfiguration configuration, ILogger<SmsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }
    
    /// <summary>
    /// SendSmsAsync methodunu çalıştırır.
    /// </summary>
    public async Task SendSmsAsync(string phone, string message)
    {
        // NetGSM entegrasyonu örneği
        var apiUrl = _configuration["Sms:ApiUrl"];
        var username = _configuration["Sms:Username"];
        var password = _configuration["Sms:Password"];
        var header = _configuration["Sms:Header"]; // Gönderici adı
        
        var xmlBody = $@"
            <?xml version='1.0' encoding='UTF-8'?>
            <mainbody>
                <header>
                    <usercode>{username}</usercode>
                    <password>{password}</password>
                    <msgheader>{header}</msgheader>
                </header>
                <body>
                    <msg><![CDATA[{message}]]></msg>
                    <no>{phone}</no>
                </body>
            </mainbody>";
        
        try
        {
            var response = await _httpClient.PostAsync(apiUrl, 
                new StringContent(xmlBody, Encoding.UTF8, "application/xml"));
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("SMS gönderildi: {Phone} - Response: {Response}", phone, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS gönderilemedi: {Phone}", phone);
            throw;
        }
    }
}
