using FluentValidation;

using TravelNest.Application.DTOs.Reviews;

namespace TravelNest.Application.Validators.Reviews;

public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}