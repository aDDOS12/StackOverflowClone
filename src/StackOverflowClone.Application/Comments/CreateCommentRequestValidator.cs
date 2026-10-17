using FluentValidation;

namespace StackOverflowClone.Application.Comments;

public sealed class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(15, 600);
    }
}
