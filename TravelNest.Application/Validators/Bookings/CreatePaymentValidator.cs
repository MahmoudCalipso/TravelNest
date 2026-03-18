using FluentValidation;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.Validators.Bookings;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking ID is required");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.PaymentLinkUrl)
            .NotEmpty().WithMessage("Payment link is required for link-based payments")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid payment link URL")
            .When(x => x.Method == PaymentMethod.PaymentLink);
    }
}

