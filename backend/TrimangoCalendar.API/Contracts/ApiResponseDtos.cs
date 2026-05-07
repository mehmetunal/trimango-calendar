using TrimangoCalendar.Core.DTOs;

namespace TrimangoCalendar.API.Contracts;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class ApiErrorResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class NotificationListResponseDto
{
    public List<NotificationDto> Data { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class WidgetEmbedResponseDto
{
    public BookingWidgetDto? Widget { get; set; }
    public string EmbedCode { get; set; } = string.Empty;
}
