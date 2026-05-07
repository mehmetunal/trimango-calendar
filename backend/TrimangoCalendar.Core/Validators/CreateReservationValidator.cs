public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad zorunludur")
            .MaximumLength(100);
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad zorunludur")
            .MaximumLength(100);
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");
        
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon zorunludur")
            .Matches(@"^[0-9+\-\s]{10,20}$").WithMessage("Geçerli bir telefon numarası giriniz");
        
        RuleFor(x => x.CheckIn)
            .NotEmpty()
            .Must(date => date.Date >= DateTime.Today)
            .WithMessage("Giriş tarihi bugünden önce olamaz");
        
        RuleFor(x => x.CheckOut)
            .NotEmpty()
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Çıkış tarihi giriş tarihinden sonra olmalıdır");
        
        RuleFor(x => x.Adults)
            .InclusiveBetween(1, 20);
        
        RuleFor(x => x.Children)
            .InclusiveBetween(0, 10);
    }
}
