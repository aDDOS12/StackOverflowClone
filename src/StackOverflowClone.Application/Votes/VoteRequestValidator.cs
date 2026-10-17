using FluentValidation;

namespace StackOverflowClone.Application.Votes;

public sealed class VoteRequestValidator : AbstractValidator<VoteRequest>
{
    public VoteRequestValidator()
    {
        RuleFor(x => x.Value)
            .Must(value => value is 1 or -1)
            .WithMessage("Vote value must be 1 or -1.");
    }
}
