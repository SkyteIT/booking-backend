using FluentValidation;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();
           

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Minimum 8 characters required")
            .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain at least one special character");

     

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");
    }
}