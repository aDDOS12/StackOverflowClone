using FluentValidation;

namespace StackOverflowClone.Application.Answers;

public sealed class CreateAnswerRequestValidator : AbstractValidator<CreateAnswerRequest>
{
    public CreateAnswerRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(30, 30000);
    }
}
