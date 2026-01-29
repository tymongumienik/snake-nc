using System.Text;
using Snake.Application.Models;

namespace Snake.Application.Core;

public class GameInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    private readonly Random _random;
    public GameConfig Config { get; }
    public int Score { get; } = 0;
    private GameCell[,] Field { get; set; }
    private readonly HashSet<(int x, int y)> _freeCells;
    public event Action<GameInstance>? GameOver;
    public bool Active { get; set; } = true;

    public GameInstance(GameConfig config, Random random)
    {
        this.Config = config;
        this.Field = new GameCell[config.GridSize, config.GridSize];
        this._random = random;

        _freeCells = [];
        for (int i = 0; i < config.GridSize; i++)
            for (int j = 0; j < config.GridSize; j++)
                _freeCells.Add((i, j));

        RandomizeFoodLocation(); // first food
    }

    private void SetCell((int x, int y) location, GameCell cell)
    {
        var (x, y) = location;

        if (x < 0 || x >= Field.GetLength(0) || y < 0 || y >= Field.GetLength(1))
            throw new ArgumentOutOfRangeException(nameof(location), "Cell is out of bounds!");

        Field[x, y] = cell;

        // returns false if not removed (we don't care)
        var _ = _freeCells.Remove(location);
    }

    private IEnumerable<(int x, int y)> SnakeCells()
    {
        for (int i = 0; i < Field.GetLength(0); ++i)
        {
            for (int j = 0; j < Field.GetLength(1); ++j)
            {
                if (Field[i, j] == GameCell.Snake)
                {
                    yield return (i, j);
                }
            }
        }
    }

    private void RandomizeFoodLocation()
    {
        if (_freeCells.Count == 0)
            GameOver?.Invoke(this);

        var index = _random.Next(_freeCells.Count);
        var location = _freeCells.ElementAt(index);

        SetCell(location, GameCell.Food);
    }

    public string Render()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Field.GetLength(0); ++i)
        {
            for (int j = 0; j < Field.GetLength(1); ++j)
            {
                builder.Append(Field[i, j].ToSymbol());
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }
}