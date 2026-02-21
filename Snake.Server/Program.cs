using Snake.Application.Core;
using Snake.Application.Repositories;
using Snake.Infrastructure.Networking;
using Snake.Infrastructure.Persistence;

namespace Snake.Server;

internal class Program
{
    static async Task Main()
    {
        IDataRepository repository = new MockDataRepository();
        GameManager manager = new(repository);

        var server = new TcpServer(manager: manager);
        await server.StartAsync(port: 4267);
    }
}

