using FluentValidation.Results;


namespace BL
{
    public class ValidationException : Exception
    {
        public List<ValidationFailure> Errors { get; }

        public ValidationException(IEnumerable<ValidationFailure> failures)
        {
            Errors = failures.ToList();
        }
    }
}
