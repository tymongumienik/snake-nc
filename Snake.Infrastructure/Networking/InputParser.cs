using Snake.Application.Models;

namespace Snake.Infrastructure.Networking;

public enum InputAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Backspace,
    Enter,
    MenuOption1,
    MenuOption2,
    Unknown
}

public static class InputParser
{
    public static InputAction Parse(byte[] slice) => slice switch
    {
        [(byte)'W' or (byte)'w'] or [0x1B, 0x5B, 0x41] => InputAction.MoveUp,
        [(byte)'S' or (byte)'s'] or [0x1B, 0x5B, 0x42] => InputAction.MoveDown,
        [(byte)'A' or (byte)'a'] or [0x1B, 0x5B, 0x44] => InputAction.MoveLeft,
        [(byte)'D' or (byte)'d'] or [0x1B, 0x5B, 0x43] => InputAction.MoveRight,
        [0x7F] => InputAction.Backspace,
        [0x0D] => InputAction.Enter,
        [0x31] => InputAction.MenuOption1,
        [0x32] => InputAction.MenuOption2,
        _ => InputAction.Unknown
    };

    public static MoveDirection? ToMoveDirection(InputAction action) => action switch
    {
        InputAction.MoveUp => MoveDirection.Up,
        InputAction.MoveDown => MoveDirection.Down,
        InputAction.MoveLeft => MoveDirection.Left,
        InputAction.MoveRight => MoveDirection.Right,
        _ => null
    };
}
