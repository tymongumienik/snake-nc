using Snake.Application.Models;

namespace Snake.Infrastructure.Entities;

public class GameResultEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public int Score { get; set; }

    public GameResult ToGameResult() => new GameResult(Score, UserName);
    public static GameResultEntity FromGameResult(GameResult result) =>
        new()
        {
            Score = result.Score,
            UserName = result.UserName
        };
}