using System.Text;
using Snake.Application.Models;

namespace Snake.Application.Core;

using Coordinate = (int x, int y);

public class GameInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    private readonly Random _random;
    public GameConfig Config { get; }
    public int Score { get; private set; } = 0;
    public LinkedList<Coordinate> Body { get; } = [];
    public Coordinate FoodLocation { get; private set; }
    public MoveDirection Direction { get; private set; } = MoveDirection.Right;

    public event Action<GameInstance>? GameOver;
    public bool Active { get; private set; } = true;
    public bool HasWon { get; private set; } = false;

    public GameInstance(GameConfig config, Random random)
    {
        Config = config;
        _random = random;

        Body.AddFirst((0, 0));

        RandomizeFoodLocation();
    }

    private HashSet<Coordinate> ComputeSnakeCells() => [.. Body];

    private void RandomizeFoodLocation()
    {
        var snakeCells = ComputeSnakeCells();
        var cells = (from x in Enumerable.Range(0, (int)Config.GridSize)
                     from y in Enumerable.Range(0, (int)Config.GridSize)
                     where !snakeCells.Contains((x, y))
                     select (x, y))
            .ToList();

        if (cells.Count == 0)
        {
            // you win!
            HasWon = true;
            Active = false;
            GameOver?.Invoke(this);
            return;
        }

        var index = _random.Next(0, cells.Count);
        FoodLocation = cells[index];
    }

    private void EndGameIfInvalid()
    {
        if (!Active) return;
        if (Body.First is null) return;

        var head = Body.First.Value;

        // head collision with wall or self (RIP)
        if (head.x < 0 || head.x >= Config.GridSize || head.y < 0 || head.y >= Config.GridSize || Body.Skip(1).Contains(head))
        {
            Active = false;
            GameOver?.Invoke(this);
            return;
        }
    }

    public void Tick(MoveDirection? move = null)
    {
        if (!Active) return;

        // can't move left when going right etc.
        // also using _move variable because LSP & compiler is too stupid for (move is not null) check
        if (move is MoveDirection _move && Direction.ContrastingDirection() != _move)
        {
            Direction = _move;
        }

        // should never happen (hopefully)
        if (Body.First is null) return;

        // do the move
        var (dx, dy) = Direction.ToMovementDelta();
        var (headX, headY) = Body.First.Value;
        var newHead = (headX + dx, headY + dy);

        // insert head at beginning (snake logic)
        Body.AddFirst(newHead);

        // eating food
        if (newHead == FoodLocation)
        {
            ++Score;
            RandomizeFoodLocation();
        }
        else
        {
            // remove tail if no food eaten (since we added a new head, this keeps the same length)
            Body.RemoveLast();
        }

        EndGameIfInvalid();
    }

    public string Render()
    {
        var snakeCells = ComputeSnakeCells();
        var sb = new StringBuilder();

        // reversed x and y (otherwise we get rotated 90 degrees :()
        for (int y = 0; y < Config.GridSize; y++)
        {
            for (int x = 0; x < Config.GridSize; x++)
            {
                if (snakeCells.Contains((x, y)))
                    sb.Append(GameCell.Snake.ToSymbol());
                else if (FoodLocation == (x, y))
                    sb.Append(GameCell.Food.ToSymbol());
                else
                    sb.Append(GameCell.Empty.ToSymbol());
            }
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
}