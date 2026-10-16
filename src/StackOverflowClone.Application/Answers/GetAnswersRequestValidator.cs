using FluentValidation;

namespace StackOverflowClone.Application.Answers;

public sealed class GetAnswersRequestValidator : AbstractValidator<GetAnswersRequest>
{
    public GetAnswersRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Sort).IsInEnum();
    }
}
