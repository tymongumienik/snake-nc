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

        Console.CursorVisible = false;

        // TODO: one Render too much when going into wall
        while (game.Active)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                MoveDirection? move = key switch
                {
                    ConsoleKey.W or ConsoleKey.UpArrow => MoveDirection.Up,
                    ConsoleKey.S or ConsoleKey.DownArrow => MoveDirection.Down,
                    ConsoleKey.A or ConsoleKey.LeftArrow => MoveDirection.Left,
                    ConsoleKey.D or ConsoleKey.RightArrow => MoveDirection.Right,
                    _ => null
                };

                game.Tick(move);
            }
            else
            {
                game.Tick();
            }

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(game.Render());
            Console.WriteLine($"Score: {game.Score}");

            Thread.Sleep(200);
        }


        Console.WriteLine(game.HasWon ? "You win!" : "Game over :(");
    }
}

