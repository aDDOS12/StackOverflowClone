using FluentValidation.TestHelper;
using StackOverflowClone.Application.Votes;

namespace StackOverflowClone.UnitTests.Application.Validators;

public sealed class VoteRequestValidatorTests
{
    private readonly VoteRequestValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public void Validate_UpvoteOrDownvote_IsValid(int value)
    {
        var result = _validator.TestValidate(new VoteRequest(value));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-5)]
    public void Validate_OtherValue_HasError(int value)
    {
        var result = _validator.TestValidate(new VoteRequest(value));

        result.ShouldHaveValidationErrorFor(x => x.Value);
    }
}
