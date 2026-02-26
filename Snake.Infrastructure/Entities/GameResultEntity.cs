using Snake.Application.Models;

namespace Snake.Infrastructure.Entities;

public class GameResultEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public int Score { get; set; }
    public uint GridSize { get; set; }

    public GameResult ToGameResult() => new(Score, UserName, GridSize);
    public static GameResultEntity FromGameResult(GameResult result) =>
        new()
        {
            Score = result.Score,
            UserName = result.UserName,
            GridSize = result.GridSize
        };
}