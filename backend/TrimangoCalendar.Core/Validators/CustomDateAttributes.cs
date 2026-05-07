using System.Reflection;

namespace TrimangoCalendar.Core.Validators;

[AttributeUsage(AttributeTargets.Property)]
public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not DateTime date) return true;
        return date.Date > DateTime.UtcNow.Date;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _otherProperty;

    public DateGreaterThanAttribute(string otherProperty)
    {
        _otherProperty = otherProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime currentDate) return ValidationResult.Success;

        var otherProp = validationContext.ObjectType.GetProperty(_otherProperty, BindingFlags.Public | BindingFlags.Instance);
        if (otherProp?.GetValue(validationContext.ObjectInstance) is DateTime otherDate)
        {
            if (currentDate <= otherDate)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} must be greater than {_otherProperty}");
            }
        }

        return ValidationResult.Success;
    }
}
