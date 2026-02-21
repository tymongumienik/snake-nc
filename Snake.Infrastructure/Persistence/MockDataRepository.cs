using Snake.Application.Models;
using Snake.Application.Repositories;

namespace Snake.Infrastructure.Persistence;

public sealed class MockDataRepository : IDataRepository
{
    public Task SaveGameResultAsync(GameResult result)
    {
        Console.WriteLine($"Game finished! Score={result.Score}, UserName={result.UserName}");
        return Task.CompletedTask;
    }

    public Task<IEnumerable<GameResult>> GetTopScoresAsync(int count)
    {
        return Task.FromResult(Enumerable.Empty<GameResult>());
    }
}