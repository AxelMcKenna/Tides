using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Tides.Infrastructure.Persistence;

namespace Tides.Integration.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public TidesDbContext CreateContext()
    {
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(_postgres.GetConnectionString());
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<TidesDbContext>()
            .UseNpgsql(dataSource)
            .Options;

        return new TidesDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync().AsTask();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
