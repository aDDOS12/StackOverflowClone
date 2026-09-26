using StackOverflowClone.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StackOverflowClone.Infrastructure.Persistence;

namespace StackOverflowClone.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<TimestampsAndSoftDeleteInterceptor>();

        builder.Services.AddDbContext<StackOverflowContext>((serviceProvider, options) =>
            options
            .UseSqlServer(builder.Configuration.GetConnectionString("StackOverflowDbConnectionString"))
            .AddInterceptors(serviceProvider.GetRequiredService<TimestampsAndSoftDeleteInterceptor>()));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}