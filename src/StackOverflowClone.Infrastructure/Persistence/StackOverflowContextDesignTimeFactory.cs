using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace StackOverflowClone.Infrastructure.Persistence;

public sealed class StackOverflowContextDesignTimeFactory : IDesignTimeDbContextFactory<StackOverflowContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__StackOverflowDbConnectionString";
    private const string LocalDevelopmentConnectionString = "Server=(localdb)\\mssqllocaldb;Database=StackOverflowDb;Trusted_Connection=true;";

    public StackOverflowContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? LocalDevelopmentConnectionString;

        var options = new DbContextOptionsBuilder<StackOverflowContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StackOverflowContext(options);
    }
}
