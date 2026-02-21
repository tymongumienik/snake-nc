using System.Text;
using Snake.Application.Core;
using Snake.Application.Models;

namespace Snake.Infrastructure.Networking;

public enum ConnectionStage
{
    EnteringName,
    ConfiguringGridSize,
    Playing,
    GameEnded
}

public class ClientSession(GameManager manager)
{
    public ConnectionStage Stage { get; private set; } = ConnectionStage.EnteringName;
    public string UserName { get; private set; } = string.Empty;
    public uint GridSize { get; private set; } = 8;
    public GameInstance? GameInstance { get; private set; }
    public object? PendingDirection;

    private GameConfig? _gameConfig;
    private CancellationTokenSource? _cancellation;

    public void HandleInput(InputAction action, byte[]? rawSlice = null)
    {
        switch (Stage)
        {
            case ConnectionStage.EnteringName:
                HandleNameEntry(action, rawSlice);
                break;
            case ConnectionStage.ConfiguringGridSize:
                HandleGridConfig(action);
                break;
            case ConnectionStage.GameEnded:
                HandleGameEnded(action);
                break;
        }
    }

    private void HandleNameEntry(InputAction action, byte[]? rawSlice)
    {
        switch (action)
        {
            case InputAction.Backspace:
                UserName = string.IsNullOrEmpty(UserName) ? UserName : UserName[..^1];
                break;
            case InputAction.Enter:
                if (GameConfig.IsValidUserName(UserName))
                    Stage = ConnectionStage.ConfiguringGridSize;
                break;
            default:
                if (rawSlice is not null && Ascii.IsValid(rawSlice))
                {
                    string newUsername = UserName + Encoding.UTF8.GetString(rawSlice);
                    if (GameConfig.IsValidUserName(newUsername))
                        UserName = newUsername;
                }
                break;
        }
    }

    private void HandleGridConfig(InputAction action)
    {
        switch (action)
        {
            case InputAction.MoveUp:
                if (GameConfig.IsValidGridSize(GridSize + 1))
                    ++GridSize;
                break;
            case InputAction.MoveDown:
                if (GameConfig.IsValidGridSize(GridSize - 1))
                    --GridSize;
                break;
            case InputAction.Enter:
                _gameConfig = new GameConfig(gridSize: GridSize, userName: UserName);
                StartNewGame();
                break;
        }
    }

    private void HandleGameEnded(InputAction action)
    {
        switch (action)
        {
            case InputAction.MenuOption1:
                StartNewGame();
                break;
            case InputAction.MenuOption2:
                Stage = ConnectionStage.EnteringName;
                break;
        }
    }

    public void StartNewGame()
    {
        if (_gameConfig is null) return;

        EndGame();
        GameInstance = manager.StartNewGame((GameConfig)_gameConfig);
        GameInstance.GameOver += (_) => EndGame();
        _cancellation = new CancellationTokenSource();
        Stage = ConnectionStage.Playing;
    }

    public void EndGame()
    {
        if (GameInstance is null) return;
        _cancellation?.Cancel();
        Interlocked.Exchange(ref PendingDirection, null);
        manager.EndGame(GameInstance.Id);
        Stage = ConnectionStage.GameEnded;
    }

    public void ApplyPendingTick()
    {
        if (Stage != ConnectionStage.Playing || GameInstance is null) return;
        var dir = (MoveDirection?)Interlocked.Exchange(ref PendingDirection, null);
        GameInstance.Tick(dir);
    }

    public CancellationToken CancellationToken => _cancellation?.Token ?? CancellationToken.None;
}
