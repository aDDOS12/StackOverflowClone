using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackOverflowClone.Infrastructure.Persistence;
using StackOverflowClone.Infrastructure.Persistence.Interceptors;

namespace StackOverflowClone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StackOverflowDbConnectionString")
            ?? throw new InvalidOperationException(
                "Connection string 'StackOverflowDbConnectionString' was not found.");

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TimestampsAndSoftDeleteInterceptor>();

        services.AddDbContext<StackOverflowContext>((serviceProvider, options) =>
            options
            .UseSqlServer(connectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<TimestampsAndSoftDeleteInterceptor>()));

        return services;
    }
}
