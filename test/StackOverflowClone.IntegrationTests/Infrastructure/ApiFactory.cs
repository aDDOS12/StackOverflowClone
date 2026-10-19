using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;
using StackOverflowClone.Api;
using StackOverflowClone.Infrastructure.Persistence;

namespace StackOverflowClone.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestDatabaseName = "StackOverflowCloneTests";

    private const string ConnectionString = $"Server=(localdb)\\mssqllocaldb;Database={TestDatabaseName};Trusted_Connection=True;";

    private Respawner _respawner = default;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:StackOverflowDbConnectionString", ConnectionString);
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-with-at-least-32-chars");
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StackOverflowContext>();

        if (context.Database.GetDbConnection().Database != TestDatabaseName)
        {
            throw new InvalidOperationException("Integration tests must run against the test database.");
        }

        await context.Database.MigrateAsync();

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new Table[] { "__EFMigrationsHistory", "Tags" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }
}
