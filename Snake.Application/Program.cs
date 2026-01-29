using Snake.Application.Core;
using Snake.Application.Models;
using Snake.Application.Repositories;

namespace Snake.Application;

internal class Program
{
    static void Main(string[] args)
    {
        IDataRepository repository = new MockDataRepository();
        GameManager manager = new(repository);

        var game = manager.StartNewGame(new GameConfig(
            gridSize: 15,
            userName: "hello"
        ));

        Console.WriteLine(game.Render());

        manager.EndGame(game.Id);
    }
}

