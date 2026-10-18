using FluentValidation;

namespace StackOverflowClone.Application.Questions;

public sealed class AcceptAnswerRequestValidator : AbstractValidator<AcceptAnswerRequest>
{
    public AcceptAnswerRequestValidator()
    {
        RuleFor(x => x.AnswerId).GreaterThan(0);
    }
}
