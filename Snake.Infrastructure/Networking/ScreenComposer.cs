using System.Text;
using Snake.Application.Rendering;

namespace Snake.Infrastructure.Networking;

public class ScreenComposer
{
    public byte[] Compose(ClientSession session)
    {
        var content = session.Stage switch
        {
            ConnectionStage.EnteringName => ComposeNameEntry(session),
            ConnectionStage.ConfiguringGridSize => ComposeGridConfig(session),
            ConnectionStage.Playing when session.GameInstance is not null => ComposeGameplay(session),
            ConnectionStage.GameEnded when session.GameInstance is not null => ComposeGameOver(session),
            _ => string.Empty
        };

        return [.. TcpMagicBytes.CLEAR_SCREEN_AND_RESET_CURSOR, .. Encoding.UTF8.GetBytes(content)];
    }

    private string ComposeNameEntry(ClientSession session)
    {
        var art = Encoding.UTF8.GetString(TcpMagicBytes.SNAKE_ART);
        return $"{art}\r\nPlease enter your name: {session.UserName}\r\nPress enter to confirm";
    }

    private string ComposeGridConfig(ClientSession session)
    {
        var art = Encoding.UTF8.GetString(TcpMagicBytes.SNAKE_ART);
        return $"{art}\r\nThe current grid size is {session.GridSize}\r\nUse W and S keys to adjust the size\r\nPress enter to start the game";
    }

    private string ComposeGameplay(ClientSession session)
    {
        return GameRenderer.Render(session.GameInstance!) + $"\r\nScore: {session.GameInstance!.Score}\r\nPress WASD or arrow keys to move";
    }

    private string ComposeGameOver(ClientSession session)
    {
        return $"Game over!\r\nScore: {session.GameInstance!.Score}\r\nPress 1 to replay with the same settings\r\nPress 2 to go back to configuration screen";
    }
}
