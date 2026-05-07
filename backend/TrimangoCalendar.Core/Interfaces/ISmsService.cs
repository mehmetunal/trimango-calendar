namespace TrimangoCalendar.Core.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(string phone, string message);
}
