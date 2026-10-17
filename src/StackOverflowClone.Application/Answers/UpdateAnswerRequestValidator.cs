using FluentValidation;

namespace StackOverflowClone.Application.Answers;

public sealed class UpdateAnswerRequestValidator : AbstractValidator<UpdateAnswerRequest>
{
    public UpdateAnswerRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(30, 30000);
    }
}
