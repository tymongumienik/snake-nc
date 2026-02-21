using Snake.Application.Core;
using Snake.Application.Models;
using Snake.Application.Rendering;
using System.Globalization;

namespace Snake.Tests;

public class GameInstanceTests
{
    [Fact]
    public void Render_ReturnsCorrectCellCount()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var rendered = GameRenderer.Render(game);
        var renderedWithoutNewlines = rendered.Replace("\r", "").Replace("\n", "");

        // Count graphemes instead of string length to account for emojis 
        Assert.Equal(8 * 8, new StringInfo(renderedWithoutNewlines).LengthInTextElements);
    }

    [Fact]
    public void Constructor_InitializesRemainingCellsAsEmpty()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var rendered = GameRenderer.Render(game);

        var emptyCount = 0;
        var charEnum = StringInfo.GetTextElementEnumerator(rendered);

        while (charEnum.MoveNext())
        {
            string grapheme = charEnum.GetTextElement();
            if (grapheme == GameCell.Empty.ToSymbol())
            {
                ++emptyCount;
            }
        }

        // Taking away 2 representing food and player
        Assert.Equal(8 * 8 - 2, emptyCount);
    }

    [Fact]
    public void Id_IsUnique()
    {
        var game1 = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var game2 = new GameInstance(new GameConfig(8, "Player2"), new Random(42));

        Assert.NotEqual(game1.Id, game2.Id);
    }

    [Fact]
    public void Score_StartsAtZero()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        Assert.Equal(0, game.Score);
    }

    [Fact]
    public void Render_OutputHasCorrectRowCount()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var renderedWithoutNewlines = new string([.. GameRenderer.Render(game).Where(c => c != '\r' && c != '\n')]);

        var length = new StringInfo(renderedWithoutNewlines).LengthInTextElements;

        Assert.Equal(8 * 8, length);
    }

    [Fact]
    public void Tick_UpdatesSnakePosition()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var initialHead = game.Body.First!.Value;

        game.Tick();

        var newHead = game.Body.First.Value;
        Assert.NotEqual(initialHead, newHead);

        // The default initial movement direction is Right
        Assert.Equal(initialHead.x + 1, newHead.x);
        Assert.Equal(initialHead.y, newHead.y);
    }

    [Fact]
    public void Tick_HitsWall_EndsGame()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        while (game.Body.First!.Value.x < game.Config.GridSize - 1)
        {
            game.Tick();
        }

        Assert.True(game.Active);

        game.Tick(); // Force cell collision to trigger game over

        Assert.False(game.Active);
    }

    [Fact]
    public void Tick_OppositeDirection_Ignored()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        // Try moving Left while moving Right (should fail)
        game.Tick(MoveDirection.Left);

        Assert.Equal(MoveDirection.Right, game.Direction);
    }

    [Fact]
    public void Tick_ValidDirection_Changed()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        game.Tick(MoveDirection.Up);

        Assert.Equal(MoveDirection.Up, game.Direction);
    }

    [Fact]
    public void GameOver_EventInvoked()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var eventRaised = false;
        game.GameOver += (g) => eventRaised = true;

        // Run into wall
        for (int i = 0; i < game.Config.GridSize; i++)
        {
            game.Tick();
        }

        Assert.True(eventRaised);
    }

    [Fact]
    public void Tick_Inactive_DoesNothing()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));
        var initialHead = game.Body.First!.Value;

        // Run into wall
        for (int i = 0; i < game.Config.GridSize; i++)
        {
            game.Tick();
        }

        Assert.False(game.Active);

        var headAfterCrash = game.Body.First!.Value;

        // Additional Tick after game ended
        game.Tick();

        var headAfterInactiveTick = game.Body.First.Value;

        Assert.Equal(headAfterCrash, headAfterInactiveTick);
    }

    [Fact]
    public void Constructor_FoodNotAtHead()
    {
        for (int i = 0; i < 100; i++)
        {
            var game = new GameInstance(new GameConfig(8, "Player1"), new Random(i));
            var head = game.Body.First!.Value;
            Assert.NotEqual(head, game.FoodLocation);
        }
    }
}
