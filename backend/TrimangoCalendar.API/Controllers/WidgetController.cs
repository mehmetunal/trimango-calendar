using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrimangoCalendar.API.Contracts;

[ApiController]
[Route("widget/api")]
[ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
[AllowAnonymous] // Herkese açık
public class WidgetController : ControllerBase
{
    private readonly IBookingEngineService _bookingEngine;
    private readonly ICurrencyService _currencyService;

    public WidgetController(IBookingEngineService bookingEngine, ICurrencyService currencyService)
    {
        _bookingEngine = bookingEngine;
        _currencyService = currencyService;
    }

    [HttpGet("config/{widgetKey}")]
    /// <summary>
    /// GetWidgetConfig methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetWidgetConfig(string widgetKey)
    {
        try
        {
            var config = await _bookingEngine.GetWidgetConfigAsync(widgetKey);
            return Ok(new { success = true, data = config });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("search/{widgetKey}")]
    /// <summary>
    /// Search methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> Search(string widgetKey, [FromBody] BookingSearchDto search)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _bookingEngine.CheckAvailabilityAsync(widgetKey, new AvailabilitySearchDto
            {
                CheckIn = search.CheckIn,
                CheckOut = search.CheckOut,
                Adults = search.Adults,
                Children = search.Children,
                CurrencyCode = search.CurrencyCode ?? "TRY"
            });

            return Ok(new { success = true, data = result });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("property/{widgetKey}")]
    /// <summary>
    /// GetProperty methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetProperty(string widgetKey)
    {
        try
        {
            var property = await _bookingEngine.GetPropertyForBookingAsync(widgetKey);
            return Ok(new { success = true, data = property });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("book/{widgetKey}")]
    /// <summary>
    /// CreateBooking methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> CreateBooking(string widgetKey, [FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reservation = await _bookingEngine.CreateBookingAsync(widgetKey, dto);

            return Ok(new
            {
                success = true,
                data = reservation,
                message = "Rezervasyonunuz başarıyla oluşturuldu!",
                reservationNumber = reservation.ReservationNumber
            });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("booking/{widgetKey}")]
    public async Task<IActionResult> GetBooking(
        string widgetKey,
        [FromQuery] string reservationNumber,
        [FromQuery] string email)
    {
        try
        {
            var booking = await _bookingEngine.GetBookingAsync(widgetKey, reservationNumber, email);
            return Ok(new { success = true, data = booking });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("cancel/{widgetKey}")]
    public async Task<IActionResult> CancelBooking(
        string widgetKey,
        [FromQuery] string reservationNumber,
        [FromQuery] string email,
        [FromBody] string reason)
    {
        try
        {
            await _bookingEngine.CancelBookingAsync(widgetKey, reservationNumber, email, reason);
            return Ok(new { success = true, message = "Rezervasyonunuz iptal edildi" });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("currencies")]
    /// <summary>
    /// GetCurrencies methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetCurrencies()
    {
        var currencies = await _currencyService.GetActiveCurrenciesAsync();
        return Ok(new { success = true, data = currencies });
    }

    [HttpGet("exchange-rate")]
    /// <summary>
    /// GetExchangeRate methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetExchangeRate([FromQuery] string from = "TRY", [FromQuery] string to = "USD")
    {
        var rate = await _currencyService.GetExchangeRateAsync(from, to);
        return Ok(new { success = true, rate, from, to });
    }
}

