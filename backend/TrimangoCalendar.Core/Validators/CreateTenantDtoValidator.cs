using FluentValidation;
using TrimangoCalendar.Core.DTOs;

namespace TrimangoCalendar.Core.Validators;

public class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
    }
}
