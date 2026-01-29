namespace Snake.Application.Models;

public readonly record struct GameConfig
{
    public uint GridSize { get; }
    public string UserName { get; }

    public GameConfig(uint gridSize, string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        if (gridSize < 4 || gridSize > 32) throw new ArgumentOutOfRangeException(nameof(gridSize));
        if (!userName.All(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x))) throw new ArgumentException(nameof(userName));

        GridSize = gridSize;
        UserName = userName;
    }
}
