using FluentValidation;

namespace Ube.Application.Features.Reviews.Validators;

public class VendorReplyDtoValidator : AbstractValidator<VendorReplyDto>
{
    public VendorReplyDtoValidator()
    {
        RuleFor(x => x.Reply)
            .NotEmpty().WithMessage("Reply is required")
            .MaximumLength(1000);
    }
}
