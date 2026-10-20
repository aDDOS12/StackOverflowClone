using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;
using StackOverflowClone.Api;
using StackOverflowClone.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace StackOverflowClone.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestDatabaseName = "StackOverflowCloneTests";

    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private string _connectionString = default!;
    private Respawner _respawner = default;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:StackOverflowDbConnectionString", _connectionString);
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-with-at-least-32-chars");
    }

    public async ValueTask InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        _connectionString = new SqlConnectionStringBuilder(_sqlContainer.GetConnectionString())
        {
            InitialCatalog = TestDatabaseName
        }.ConnectionString;

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StackOverflowContext>();

        if (context.Database.GetDbConnection().Database != TestDatabaseName)
        {
            throw new InvalidOperationException("Integration tests must run against the test database.");
        }

        await context.Database.MigrateAsync();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new Table[] { "__EFMigrationsHistory", "Tags" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }
}
