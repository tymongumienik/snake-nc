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
        [(byte)'W' or (byte)'w'] => InputAction.MoveUp,
        [(byte)'S' or (byte)'s'] => InputAction.MoveDown,
        [(byte)'A' or (byte)'a'] => InputAction.MoveLeft,
        [(byte)'D' or (byte)'d'] => InputAction.MoveRight,
        // 0x7F is backspace on Linux, 0x08 is backspace on Windows
        [0x7F] or [0x08] => InputAction.Backspace,
        // CRLF/CR/LF support
        [0x0D] or [0x0A] or [0x0D, 0x0A] => InputAction.Enter,
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
