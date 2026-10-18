using FluentValidation.TestHelper;
using StackOverflowClone.Application.Questions;

namespace StackOverflowClone.UnitTests.Application.Validators;

public sealed class CreateQuestionRequestValidatorTests
{
    private static readonly CreateQuestionRequest ValidRequest = new(
        "How to filter a list with LINQ?",
        "I have a list of numbers and want to keep only the even ones. What is the idiomatic way?",
        ["c#", "linq"]);

    private readonly CreateQuestionRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullTags_ReturnsErrorInsteadOfThrowing()
    {
        var result = _validator.TestValidate(ValidRequest with { Tags = null! });

        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_EmptyTags_HasError()
    {
        var result = _validator.TestValidate(ValidRequest with { Tags = [] });

        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_MoreThanFiveTags_HasError()
    {
        var result = _validator.TestValidate(ValidRequest with { Tags = ["a", "b", "c", "d", "e", "f"] });

        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_EmptyTagInList_HasErrorForThatItem()
    {
        var result = _validator.TestValidate(ValidRequest with { Tags = ["c#", ""] });

        result.ShouldHaveValidationErrorFor("Tags[1]");
    }
}
