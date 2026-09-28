using FluentValidation.Results;

namespace StackOverflowClone.Application.Common.Exceptions;
public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationFailure> failures) : base("One or more validation errors occured.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}
