using Snake.Application.Core;
using Snake.Application.Models;
using Snake.Application.Rendering;
using System.Globalization;

namespace Snake.Tests;

public class GameRendererTests
{
    [Fact]
    public void Render_ContainsSnakeSymbol()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        var rendered = GameRenderer.Render(game);

        Assert.Contains(GameCell.Snake.ToSymbol(), rendered);
    }

    [Fact]
    public void Render_ContainsFoodSymbol()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        var rendered = GameRenderer.Render(game);

        Assert.Contains(GameCell.Food.ToSymbol(), rendered);
    }

    [Fact]
    public void Render_ContainsEmptySymbol()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        var rendered = GameRenderer.Render(game);

        Assert.Contains(GameCell.Empty.ToSymbol(), rendered);
    }

    [Fact]
    public void Render_DifferentGridSizes_ProduceCorrectCellCount()
    {
        foreach (uint size in new uint[] { 4, 8, 16, 32 })
        {
            var game = new GameInstance(new GameConfig(size, "Player1"), new Random(42));
            var rendered = GameRenderer.Render(game);
            var renderedWithoutNewlines = rendered.Replace("\r", "").Replace("\n", "");

            Assert.Equal((int)(size * size), new StringInfo(renderedWithoutNewlines).LengthInTextElements);
        }
    }

    [Fact]
    public void Render_AfterTick_ReflectsMovement()
    {
        var game = new GameInstance(new GameConfig(8, "Player1"), new Random(42));

        var before = GameRenderer.Render(game);
        game.Tick();
        var after = GameRenderer.Render(game);

        Assert.NotEqual(before, after);
    }
}
