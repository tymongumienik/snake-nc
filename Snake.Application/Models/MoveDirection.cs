namespace Snake.Application.Models;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,
}

public static class MoveDirectionExtensions
{
    public static (int dx, int dy) ToMovementDelta(this MoveDirection direction)
    {
        return direction switch
        {
            MoveDirection.Up => (0, -1),
            MoveDirection.Down => (0, 1),
            MoveDirection.Left => (-1, 0),
            MoveDirection.Right => (1, 0),
            _ => throw new NotImplementedException(),
        };
    }

    public static MoveDirection ContrastingDirection(this MoveDirection direction)
    {
        return direction switch
        {
            MoveDirection.Up => MoveDirection.Down,
            MoveDirection.Down => MoveDirection.Up,
            MoveDirection.Left => MoveDirection.Right,
            MoveDirection.Right => MoveDirection.Left,
            _ => throw new NotImplementedException(),
        };
    }
}