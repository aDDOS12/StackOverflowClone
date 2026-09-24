
using StackOverflowClone.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace StackOverflowClone.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<StackOverflowContext>(
            option => option
            .UseSqlServer(builder.Configuration.GetConnectionString("StackOverflowDbConnectionString"))
            );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}
