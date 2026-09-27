using Microsoft.Extensions.DependencyInjection;
using StackOverflowClone.Application.Tags;

namespace StackOverflowClone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITagService, TagService>();
        return services;
    }
}
