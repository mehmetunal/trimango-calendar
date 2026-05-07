public class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    private Money() { } // EF Core için
    
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Para tutarı negatif olamaz");
            
        Amount = Math.Round(amount, currency?.DecimalPlaces ?? 2);
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }
    
    // Para birimi dönüşümü
    public Money ConvertTo(Currency targetCurrency, decimal exchangeRate)
    {
        if (Currency.Code == targetCurrency.Code)
            return this;
            
        var convertedAmount = Amount * exchangeRate;
        return new Money(convertedAmount, targetCurrency);
    }
    
    // Operatör overload'ları
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency.Code != b.Currency.Code)
            throw new InvalidOperationException("Farklı para birimleri toplanamaz");
            
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }
    
    public override string ToString()
    {
        return $"{Amount:N2} {Currency.Code}";
    }
    
    public string ToFormattedString()
    {
        return $"{Currency.Symbol}{Amount:N2}";
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency.Code;
    }
}

