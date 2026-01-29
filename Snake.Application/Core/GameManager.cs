using Snake.Application.Models;
using Snake.Application.Repositories;

namespace Snake.Application.Core;

public class GameManager(IDataRepository repository)
{
    private readonly Dictionary<Guid, GameInstance> _activeGames = [];
    private readonly Random _random = new();

    public GameInstance StartNewGame(GameConfig config)
    {
        var game = new GameInstance(config, _random);
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
            var result = new GameResult
            {
                Score = game.Score,
                UserName = game.Config.UserName,
            };
            repository.SaveGameResult(result);
            _activeGames.Remove(gameId);
        }
    }

    public IReadOnlyCollection<GameInstance> ActiveGames => _activeGames.Values;
}
