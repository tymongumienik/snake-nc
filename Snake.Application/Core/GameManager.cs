using System.Collections.Concurrent;
using Snake.Application.Models;
using Snake.Application.Repositories;

namespace Snake.Application.Core;

public class GameManager(IDataRepository repository)
{
    private readonly ConcurrentDictionary<Guid, GameInstance> _activeGames = [];

    public GameInstance StartNewGame(GameConfig config)
    {
        var game = new GameInstance(config, Random.Shared);
        _activeGames[game.Id] = game;
        game.GameOver += (game) => EndGame(game.Id);
        return game;
    }

    public GameInstance? GetGame(Guid gameId)
    {
        _activeGames.TryGetValue(gameId, out var game);
        return game;
    }

    public void EndGame(Guid gameId)
    {
        if (_activeGames.TryGetValue(gameId, out var game))
        {
            game.Active = false;
            var result = new GameResult(game.Score, game.Config.UserName, game.Config.GridSize);

            try
            {
                repository.SaveGameResultAsync(result).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save game result: {ex.Message}");
            }

            _activeGames.TryRemove(gameId, out _);
        }
    }

    public IReadOnlyCollection<GameInstance> ActiveGames => [.. _activeGames.Values];
}
