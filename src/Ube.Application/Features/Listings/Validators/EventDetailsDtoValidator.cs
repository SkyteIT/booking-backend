using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class EventDetailsDtoValidator : AbstractValidator<EventDetailsDto>
{
    public EventDetailsDtoValidator()
    {
        RuleFor(x => x.EventName)
            .NotEmpty().WithMessage("Event name is required");

        RuleFor(x => x.Organizer)
            .NotEmpty().WithMessage("Organizer is required");

        RuleFor(x => x.DateAndTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future");

        RuleFor(x => x.SeatCount)
            .GreaterThan(0).WithMessage("Seat count must be greater than 0");

        RuleFor(x => x.TicketPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Ticket price cannot be negative");

        RuleForEach(x => x.TicketTypes)
            .ChildRules(ticket =>
            {
                ticket.RuleFor(t => t.Type).NotEmpty().WithMessage("Ticket type is required");
                ticket.RuleFor(t => t.Quantity).GreaterThan(0).WithMessage("Ticket quantity must be greater than 0");
                ticket.RuleFor(t => t.Price).GreaterThanOrEqualTo(0).WithMessage("Ticket price cannot be negative");
            });
    }
}
