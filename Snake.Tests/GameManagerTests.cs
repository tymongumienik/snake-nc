using Snake.Application.Core;
using Snake.Application.Models;
using Snake.Tests.Helpers;

namespace Snake.Tests;

public class GameManagerTests
{
    [Fact]
    public void StartNewGame_ReturnsGameInstance()
    {
        var manager = new GameManager(new TestDataRepository());
        var config = new GameConfig(8, "Player1");

        var game = manager.StartNewGame(config);

        Assert.NotNull(game);
        Assert.IsType<GameInstance>(game);
    }

    [Fact]
    public void StartNewGame_AddsGameToActiveGames()
    {
        var manager = new GameManager(new TestDataRepository());
        var config = new GameConfig(8, "Player1");

        var game = manager.StartNewGame(config);

        Assert.Contains(game, manager.ActiveGames);
    }

    [Fact]
    public void GetGame_ReturnsCorrectGame()
    {
        var manager = new GameManager(new TestDataRepository());
        var config = new GameConfig(8, "Player1");
        var game = manager.StartNewGame(config);

        var retrieved = manager.GetGame(game.Id);

        Assert.Same(game, retrieved);
    }

    [Fact]
    public void GetGame_ReturnsNullForUnknownId()
    {
        var manager = new GameManager(new TestDataRepository());

        var result = manager.GetGame(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void EndGame_RemovesGameFromActiveGames()
    {
        var manager = new GameManager(new TestDataRepository());
        var config = new GameConfig(8, "Player1");
        var game = manager.StartNewGame(config);

        manager.EndGame(game.Id);

        Assert.DoesNotContain(game, manager.ActiveGames);
    }

    [Fact]
    public void EndGame_SetsActiveToFalse()
    {
        var manager = new GameManager(new TestDataRepository());
        var config = new GameConfig(8, "Player1");
        var game = manager.StartNewGame(config);

        manager.EndGame(game.Id);

        Assert.False(game.Active);
    }

    [Fact]
    public void EndGame_CallsRepositorySaveGameResult()
    {
        var repository = new TestDataRepository();
        var manager = new GameManager(repository);
        var config = new GameConfig(8, "TestPlayer");
        var game = manager.StartNewGame(config);

        manager.EndGame(game.Id);

        Assert.Single(repository.SavedResults);
        Assert.Equal("TestPlayer", repository.SavedResults[0].UserName);
    }

    [Fact]
    public void EndGame_NoOpForUnknownId()
    {
        var repository = new TestDataRepository();
        var manager = new GameManager(repository);

        // Verify that attempting to end a non-existent game does not throw an exception.
        manager.EndGame(Guid.NewGuid());

        Assert.Empty(repository.SavedResults);
    }

    [Fact]
    public void ActiveGames_ReturnsAllActiveGames()
    {
        var manager = new GameManager(new TestDataRepository());
        var game1 = manager.StartNewGame(new GameConfig(8, "Player1"));
        var game2 = manager.StartNewGame(new GameConfig(8, "Player2"));

        Assert.Equal(2, manager.ActiveGames.Count);
        Assert.Contains(game1, manager.ActiveGames);
        Assert.Contains(game2, manager.ActiveGames);
    }

    [Fact]
    public void MultipleGames_HaveUniqueIds()
    {
        var manager = new GameManager(new TestDataRepository());
        var game1 = manager.StartNewGame(new GameConfig(8, "Player1"));
        var game2 = manager.StartNewGame(new GameConfig(8, "Player2"));

        Assert.NotEqual(game1.Id, game2.Id);
    }
}
