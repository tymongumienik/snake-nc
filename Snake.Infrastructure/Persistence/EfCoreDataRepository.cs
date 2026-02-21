using Microsoft.EntityFrameworkCore;
using Snake.Application.Models;
using Snake.Application.Repositories;
using Snake.Infrastructure.Entities;

namespace Snake.Infrastructure.Persistence;

public sealed class EfCoreDataRepository(SnakeDbContext context) : IDataRepository
{
    public async Task SaveGameResultAsync(GameResult result)
    {
        var entity = GameResultEntity.FromGameResult(result);
        context.GameResults.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GameResult>> GetTopScoresAsync(int count)
    {
        return await context.GameResults
            .OrderByDescending(r => r.Score)
            .Take(count)
            .Select(r => r.ToGameResult())
            .ToListAsync();
    }
}
