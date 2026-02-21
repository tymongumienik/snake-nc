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
    public bool Active { get; internal set; } = true;
    public bool HasWon { get; private set; } = false;

    public GameInstance(GameConfig config, Random random)
    {
        Config = config;
        _random = random;

        Body.AddFirst(((int)(config.GridSize / 2), (int)(config.GridSize / 2)));

        RandomizeFoodLocation();
    }

    private void RandomizeFoodLocation()
    {
        var snakeCells = new HashSet<Coordinate>(Body);
        var cells = (from x in Enumerable.Range(0, (int)Config.GridSize)
                     from y in Enumerable.Range(0, (int)Config.GridSize)
                     where !snakeCells.Contains((x, y))
                     select (x, y))
            .ToList();

        if (cells.Count == 0)
        {
            EndGame(true);
            return;
        }

        var index = _random.Next(0, cells.Count);
        FoodLocation = cells[index];
    }

    private void EndGame(bool won = false)
    {
        Active = false;
        HasWon = won;
        GameOver?.Invoke(this);
    }

    private void EndGameIfInvalid()
    {
        if (!Active) return;
        if (Body.First is null) return;

        var head = Body.First.Value;

        if (IsHeadInvalid(head))
        {
            EndGame();
        }
    }

    private bool IsHeadInvalid(Coordinate head)
    {
        return head.x < 0 || head.x >= Config.GridSize || head.y < 0 || head.y >= Config.GridSize || Body.Skip(1).Contains(head);
    }

    public void Tick(MoveDirection? move = null)
    {
        if (!Active) return;

        if (move is MoveDirection _move && Direction.ContrastingDirection() != _move)
        {
            Direction = _move;
        }

        if (Body.First is null) return;

        var (dx, dy) = Direction.ToMovementDelta();
        var (headX, headY) = Body.First.Value;
        var newHead = (headX + dx, headY + dy);

        if (IsHeadInvalid(newHead))
        {
            EndGame();
            return;
        }

        Body.AddFirst(newHead);

        if (newHead == FoodLocation)
        {
            ++Score;
            RandomizeFoodLocation();
        }
        else
        {
            Body.RemoveLast();
        }

        EndGameIfInvalid();
    }
}