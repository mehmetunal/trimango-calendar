using FluentValidation;
using TrimangoCalendar.Core.DTOs;

namespace TrimangoCalendar.Core.Validators;

public class GrantAuthorizationDtoValidator : AbstractValidator<GrantAuthorizationDto>
{
    public GrantAuthorizationDtoValidator()
    {
        RuleFor(x => x.AgencyId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.Level).IsInEnum();
        RuleFor(x => x.CustomCommissionRate)
            .InclusiveBetween(0, 100)
            .When(x => x.CustomCommissionRate.HasValue);
        RuleFor(x => x.MaxMarkupRate)
            .InclusiveBetween(0, 100)
            .When(x => x.MaxMarkupRate.HasValue);
        RuleFor(x => x.DefaultMarkupRate)
            .InclusiveBetween(0, 100)
            .When(x => x.DefaultMarkupRate.HasValue);
    }
}
