using FluentValidation;
using TravelNest.Application.DTOs.Bookings;

namespace TravelNest.Application.Validators.Bookings;

public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.CheckInDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Check-in date must be today or in the future");

        RuleFor(x => x.CheckOutDate)
            .GreaterThan(x => x.CheckInDate)
            .WithMessage("Check-out date must be after check-in date");

        RuleFor(x => x.NumberOfGuests)
            .GreaterThan(0).WithMessage("Number of guests must be at least 1");
    }
}
