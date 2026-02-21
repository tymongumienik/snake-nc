using Snake.Application.Models;

namespace Snake.Application.Repositories;

public interface IDataRepository
{
    Task SaveGameResultAsync(GameResult result);
    Task<IEnumerable<GameResult>> GetTopScoresAsync(int count);
}