using FluentValidation;

public class BlockDatesValidator : AbstractValidator<BlockDatesRequest>
{
    public BlockDatesValidator()
    {
        RuleFor(x => x.Dates)
            .NotEmpty()
            .WithMessage("Dates are required");

        RuleForEach(x => x.Dates)
            .Must(d => d.Date >= DateTime.UtcNow.Date)
            .WithMessage("Cannot block past dates");
    }
}