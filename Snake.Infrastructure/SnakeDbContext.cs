using Microsoft.EntityFrameworkCore;
using Snake.Infrastructure.Entities;

namespace Snake.Infrastructure;

public class SnakeDbContext(DbContextOptions<SnakeDbContext> options)
    : DbContext(options)
{
    public DbSet<GameResultEntity> GameResults { get; set; } = null!;
}