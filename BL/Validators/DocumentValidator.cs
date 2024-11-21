using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SharedResources.Entities;

namespace BL.Validators
{
    public class DocumentValidator : AbstractValidator<DocumentBl>
    {
        public DocumentValidator()
        {
            RuleFor(doc => doc.Id).NotEmpty().WithMessage("Id is required.");

            RuleFor(doc => doc.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MinimumLength(5).WithMessage("Title must be at least 5 characters long.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(doc => doc.Filepath)
            .NotEmpty().WithMessage("Filepath is required.")
            .MinimumLength(10).WithMessage("Filepath must be at least 10 characters long.")
            .MaximumLength(120).WithMessage("Filepath cannot be longer than 120 characters.");

            RuleFor(doc => doc.CreatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedAt cannot be in the future.");

            RuleFor(doc => doc.UpdatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("UpdatedAt cannot be in the future.");

        }
    }
}
