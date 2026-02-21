using Snake.Application.Core;
using Snake.Application.Models;
using Snake.Infrastructure.Networking;
using Snake.Tests.Helpers;

namespace Snake.Tests;

public class ClientSessionTests
{
    [Fact]
    public void InitialStage_IsEnteringName()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));

        Assert.Equal(ConnectionStage.EnteringName, session.Stage);
    }

    [Fact]
    public void HandleInput_EnterWithValidName_GoesToConfiguringStage()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));

        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);

        Assert.Equal(ConnectionStage.ConfiguringGridSize, session.Stage);
    }

    [Fact]
    public void HandleInput_EnterWithEmptyName_StaysOnEnteringName()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));

        session.HandleInput(InputAction.Enter);

        Assert.Equal(ConnectionStage.EnteringName, session.Stage);
    }

    [Fact]
    public void HandleInput_Backspace_RemovesLastCharacter()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));

        session.HandleInput(InputAction.Unknown, [(byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o']);
        session.HandleInput(InputAction.Backspace);

        Assert.Equal("Hell", session.UserName);
    }

    [Fact]
    public void HandleInput_GridSizeUp_IncreasesGridSize()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);

        var initialSize = session.GridSize;
        session.HandleInput(InputAction.MoveUp);

        Assert.Equal(initialSize + 1, session.GridSize);
    }

    [Fact]
    public void HandleInput_GridSizeDown_DecreasesGridSize()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);

        var initialSize = session.GridSize;
        session.HandleInput(InputAction.MoveDown);

        Assert.Equal(initialSize - 1, session.GridSize);
    }

    [Fact]
    public void HandleInput_EnterOnConfig_StartsGame()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);
        session.HandleInput(InputAction.Enter);

        Assert.Equal(ConnectionStage.Playing, session.Stage);
        Assert.NotNull(session.GameInstance);
    }

    [Fact]
    public void EndGame_SetsStageToGameEnded()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);
        session.HandleInput(InputAction.Enter);

        session.EndGame();

        Assert.Equal(ConnectionStage.GameEnded, session.Stage);
    }

    [Fact]
    public void HandleInput_MenuOption1_Replays()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);
        session.HandleInput(InputAction.Enter);
        session.EndGame();

        session.HandleInput(InputAction.MenuOption1);

        Assert.Equal(ConnectionStage.Playing, session.Stage);
    }

    [Fact]
    public void HandleInput_MenuOption2_GoesBackToNameEntry()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);
        session.HandleInput(InputAction.Enter);
        session.EndGame();

        session.HandleInput(InputAction.MenuOption2);

        Assert.Equal(ConnectionStage.EnteringName, session.Stage);
    }

    [Fact]
    public void ApplyPendingTick_WhenPlaying_TicksGame()
    {
        var session = new ClientSession(new GameManager(new TestDataRepository()));
        session.HandleInput(InputAction.Unknown, [(byte)'T', (byte)'e', (byte)'s', (byte)'t']);
        session.HandleInput(InputAction.Enter);
        session.HandleInput(InputAction.Enter);

        var initialHead = session.GameInstance!.Body.First!.Value;
        session.ApplyPendingTick();
        var newHead = session.GameInstance.Body.First!.Value;

        Assert.NotEqual(initialHead, newHead);
    }
}
