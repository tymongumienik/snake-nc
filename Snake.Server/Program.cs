using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Snake.Application.Core;
using Snake.Application.Repositories;
using Snake.Infrastructure;
using Snake.Infrastructure.Networking;
using Snake.Infrastructure.Persistence;

namespace Snake.Server;

internal class Program
{
    static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var connectionString = configuration.GetConnectionString("SnakeDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string for the database not found. Check GitHub README for instructions.");
        }

        var factory = new PooledDbContextFactory<SnakeDbContext>(
            new DbContextOptionsBuilder<SnakeDbContext>()
                .UseSqlite(connectionString)
                .Options
            );

        using (var context = await factory.CreateDbContextAsync())
        {
            await context.Database.MigrateAsync();
        }

        IDataRepository repository = new EfCoreDataRepository(factory);
        GameManager manager = new(repository);

        var server = new TcpServer(manager: manager);
        await server.StartAsync(port: 4267);
    }
}

