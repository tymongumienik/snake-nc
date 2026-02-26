using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Snake.Infrastructure;

class SnakeDbContextFactory : IDesignTimeDbContextFactory<SnakeDbContext>
{
    public SnakeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SnakeDbContext>()
            .Build();

        var connectionString = configuration.GetConnectionString("SnakeDb");

        return new SnakeDbContext(new DbContextOptionsBuilder<SnakeDbContext>()
            .UseSqlite(connectionString)
            .Options);
    }
}