using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StackOverflowClone.Application.Answers;
using StackOverflowClone.Application.Auth;
using StackOverflowClone.Application.Comments;
using StackOverflowClone.Application.Questions;
using StackOverflowClone.Application.Tags;
using StackOverflowClone.Application.Users;
using StackOverflowClone.Application.Votes;

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
        services.AddScoped<IAnswerService, AnswerService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IVoteService, VoteService>();
        return services;
    }
}
