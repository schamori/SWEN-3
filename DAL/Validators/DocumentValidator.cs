using FluentValidation;
using DAL.DTO;

namespace DAL.Validators
{
    public class DocumentValidator : AbstractValidator<CreateDocumentDTO>
    {
        public DocumentValidator()
        {
            RuleFor(doc => doc.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MinimumLength(5).WithMessage("Title must be at least 5 characters long.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(doc => doc.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MinimumLength(10).WithMessage("Content must be at least 10 characters long.");
        }
    }
}