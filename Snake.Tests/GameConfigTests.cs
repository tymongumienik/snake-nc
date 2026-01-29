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
}
