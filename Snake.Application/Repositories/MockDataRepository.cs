using Snake.Application.Models;

namespace Snake.Application.Repositories;

public class MockDataRepository : IDataRepository
{
    public void SaveGameResult(GameResult result)
    {
        Console.WriteLine($"Game finished! Score={result.Score}, UserName={result.UserName}");
    }
}