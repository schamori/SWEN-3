using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;


namespace BL
{
    [ExcludeFromCodeCoverage]
    public class ValidationException : Exception
    {
        public List<ValidationFailure> Errors { get; }

        public ValidationException(IEnumerable<ValidationFailure> failures)
        {
            Errors = failures.ToList();
        }
    }
}
