public interface IPricingService
{
    Task<PriceCalculationResult> CalculatePriceAsync(PriceCalculationRequest request);
    Task<decimal> GetDailyPriceAsync(Guid unitId, DateTime date, string currencyCode);
    Task<List<DailyPrice>> GetDailyPricesAsync(Guid unitId, DateTime checkIn, DateTime checkOut, string currencyCode);
    Task<bool> ValidatePromoCodeAsync(string promoCode, Guid unitId, DateTime checkIn, DateTime checkOut);
}

