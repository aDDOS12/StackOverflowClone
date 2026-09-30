using FluentValidation;

namespace StackOverflowClone.Application.Questions;

public sealed class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(15, 150);
        RuleFor(x => x.Content).NotEmpty().Length(30, 30000);
        RuleFor(x => x.Tags)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one tag is required.")
            .Must(tags => tags.Count <= 5)
            .WithMessage("A question can have maximum number of 5 tags.");
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(35);
    }
}
