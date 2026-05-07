using FluentValidation;
using TrimangoCalendar.Core.DTOs;

namespace TrimangoCalendar.Core.Validators;

public class CreateUnitDtoValidator : AbstractValidator<CreateUnitDto>
{
    public CreateUnitDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.MaxAdults).InclusiveBetween(1, 20);
        RuleFor(x => x.MaxChildren).InclusiveBetween(0, 10);
        RuleFor(x => x.MaxInfants).InclusiveBetween(0, 5);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
    }
}
