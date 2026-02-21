using Snake.Application.Models;
using Snake.Application.Repositories;

namespace Snake.Tests.Helpers
{
    public sealed class TestDataRepository : IDataRepository
    {
        public List<GameResult> SavedResults { get; } = [];

        public Task SaveGameResultAsync(GameResult result)
        {
            SavedResults.Add(result);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<GameResult>> GetTopScoresAsync(int count)
        {
            return Task.FromResult(SavedResults.OrderByDescending(r => r.Score).Take(count));
        }
    }
}
