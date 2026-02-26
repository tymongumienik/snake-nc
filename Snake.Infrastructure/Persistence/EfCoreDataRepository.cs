using Microsoft.EntityFrameworkCore;
using Snake.Application.Models;
using Snake.Application.Repositories;
using Snake.Infrastructure.Entities;

namespace Snake.Infrastructure.Persistence;

public sealed class EfCoreDataRepository(IDbContextFactory<SnakeDbContext> factory) : IDataRepository
{
    public async Task SaveGameResultAsync(GameResult result)
    {
        await using var context = await factory.CreateDbContextAsync();

        var entity = GameResultEntity.FromGameResult(result);
        context.GameResults.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GameResult>> GetTopScoresAsync(int count)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.GameResults
            .OrderByDescending(r => r.Score)
            .Take(count)
            .Select(r => r.ToGameResult())
            .ToListAsync();
    }
}
