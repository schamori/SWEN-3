using FluentValidation;
using SharedResources.Entities;

namespace SharedResources.Validators
{
    public class DocumentValidator : AbstractValidator<DocumentBl>
    {
        public DocumentValidator()
        {
            RuleFor(doc => doc.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MinimumLength(5).WithMessage("Title must be at least 5 characters long.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        }
    }
}