using Snake.Application.Models;

namespace Snake.Tests;

public class GameConfigTests
{
    [Fact]
    public void Constructor_ThrowsOnInvalidUsername()
    {
        Assert.Throws<ArgumentException>(() => new GameConfig(userName: "!!😵‍💫 Test 😵‍💫!!", gridSize: 8));
    }

    [Fact]
    public void Constructor_ThrowsOnTooSmallGridSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameConfig(userName: "Test", gridSize: 1));
    }

    [Fact]
    public void Constructor_ThrowsOnTooLargeGridSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameConfig(userName: "Test", gridSize: 64));
    }

    [Fact]
    public void Constructor_CorrectlyInitializes()
    {
        var config = new GameConfig(userName: "Player1", gridSize: 8);
        Assert.Equal("Player1", config.UserName);
        Assert.Equal(8u, config.GridSize);
    }

    [Fact]
    public void Constructor_AcceptsUsernameWithSpaces()
    {
        var config = new GameConfig(gridSize: 8, userName: "Player One");
        Assert.Equal("Player One", config.UserName);
    }

    [Fact]
    public void Constructor_ThrowsOnEmptyUsername()
    {
        Assert.Throws<ArgumentException>(() => new GameConfig(gridSize: 8, userName: ""));
    }

    [Fact]
    public void Constructor_ThrowsOnWhitespaceOnlyUsername()
    {
        Assert.Throws<ArgumentException>(() => new GameConfig(gridSize: 8, userName: "   "));
    }
}
