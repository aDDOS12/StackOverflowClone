using FluentValidation.TestHelper;
using StackOverflowClone.Application.Users;

namespace StackOverflowClone.UnitTests.Application.Validators;

public sealed class RegisterUserRequestValidatorTests
{
    private static readonly RegisterUserRequest ValidRequest = new("jan_kowalski", "jan@example.com", "Haslo1234");

    private readonly RegisterUserRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Łukasz")]
    [InlineData("anna.nowak")]
    [InlineData("user-123")]
    public void Validate_UsernameWithAllowedCharacters_IsValid(string username)
    {
        var result = _validator.TestValidate(ValidRequest with { Username = username });

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("jan kowalski")]
    [InlineData("jan!")]
    public void Validate_InvalidUsername_HasError(string username)
    {
        var result = _validator.TestValidate(ValidRequest with { Username = username });

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("Short1")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validate_WeakPassword_HasError(string password)
    {
        var result = _validator.TestValidate(ValidRequest with { Password = password });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
