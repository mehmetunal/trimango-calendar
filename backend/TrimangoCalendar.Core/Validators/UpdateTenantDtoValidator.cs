using FluentValidation;
using TrimangoCalendar.Core.DTOs;

namespace TrimangoCalendar.Core.Validators;

public class UpdateTenantDtoValidator : AbstractValidator<UpdateTenantDto>
{
    public UpdateTenantDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
    }
}
