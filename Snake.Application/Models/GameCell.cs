namespace Snake.Application.Models;

public enum GameCell
{
    Empty,
    Snake,
    Food,
};

static class GameCellExtensions
{
    public static string ToSymbol(this GameCell cell)
    {
        return cell switch
        {
            GameCell.Empty => "⏹",
            GameCell.Snake => "🟩",
            GameCell.Food => "🍎",
            _ => throw new NotImplementedException(),
        };
    }
}