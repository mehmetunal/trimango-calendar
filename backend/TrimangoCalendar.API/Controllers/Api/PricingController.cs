[ApiController]
[Route("api/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricingService;
    private readonly ICurrencyService _currencyService;
    private readonly ISeasonRateService _seasonRateService;
    
    public PricingController(
        IPricingService pricingService,
        ICurrencyService currencyService,
        ISeasonRateService seasonRateService)
    {
        _pricingService = pricingService;
        _currencyService = currencyService;
        _seasonRateService = seasonRateService;
    }
    
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequest request)
    {
        try
        {
            var result = await _pricingService.CalculatePriceAsync(request);
            return Ok(new { success = true, data = result });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies()
    {
        var currencies = await _currencyService.GetActiveCurrenciesAsync();
        return Ok(new { success = true, data = currencies });
    }
    
    [HttpGet("exchange-rate")]
    public async Task<IActionResult> GetExchangeRate(
        [FromQuery] string from = "TRY", 
        [FromQuery] string to = "USD")
    {
        try
        {
            var rate = await _currencyService.GetExchangeRateAsync(from, to);
            return Ok(new 
            { 
                success = true, 
                data = new 
                { 
                    baseCurrency = from, 
                    targetCurrency = to, 
                    rate,
                    date = DateTime.Today.ToString("yyyy-MM-dd")
                } 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("daily-prices/{unitId}")]
    public async Task<IActionResult> GetDailyPrices(
        Guid unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] string currency = "TRY")
    {
        try
        {
            var prices = await _pricingService.GetDailyPricesAsync(unitId, checkIn, checkOut, currency);
            return Ok(new { success = true, data = prices });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("seasons")]
    [Authorize]
    public async Task<IActionResult> CreateSeasonRate([FromBody] CreateSeasonRateDto dto)
    {
        try
        {
            var seasonRate = await _seasonRateService.CreateAsync(dto);
            return Ok(new { success = true, data = seasonRate });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("seasons/{unitId}")]
    public async Task<IActionResult> GetSeasonRates(Guid unitId)
    {
        var seasons = await _seasonRateService.GetByUnitAsync(unitId);
        return Ok(new { success = true, data = seasons });
    }
}
