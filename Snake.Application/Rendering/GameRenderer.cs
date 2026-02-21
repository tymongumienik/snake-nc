using System.Text;
using Snake.Application.Core;
using Snake.Application.Models;

namespace Snake.Application.Rendering;

using Coordinate = (int x, int y);

public static class GameRenderer
{
    public static string Render(GameInstance game)
    {
        var snakeCells = new HashSet<Coordinate>(game.Body);
        var sb = new StringBuilder();

        for (int y = 0; y < game.Config.GridSize; y++)
        {
            for (int x = 0; x < game.Config.GridSize; x++)
            {
                if (snakeCells.Contains((x, y)))
                    sb.Append(GameCell.Snake.ToSymbol());
                else if (game.FoodLocation == (x, y))
                    sb.Append(GameCell.Food.ToSymbol());
                else
                    sb.Append(GameCell.Empty.ToSymbol());
            }
            sb.Append("\r\n");
        }

        return sb.ToString().TrimEnd();
    }
}
