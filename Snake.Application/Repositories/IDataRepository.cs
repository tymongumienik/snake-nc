using Snake.Application.Models;

namespace Snake.Application.Repositories;

public interface IDataRepository
{
    void SaveGameResult(GameResult progress);
}