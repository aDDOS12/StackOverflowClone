using FluentValidation;

namespace StackOverflowClone.Application.Questions;

public class GetQuestionRequestValidator : AbstractValidator<GetQuestionRequest>
{
    public GetQuestionRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.Tag).MaximumLength(35);
    }
}
