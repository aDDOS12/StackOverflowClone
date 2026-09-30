using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StackOverflowClone.Application.Auth;
using StackOverflowClone.Application.Questions;
using StackOverflowClone.Application.Tags;
using StackOverflowClone.Application.Users;

namespace StackOverflowClone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITagService, TagService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuestionService, QuestionService>();
        return services;
    }
}
