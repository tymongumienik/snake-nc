using System.Net;
using System.Net.Sockets;
using Snake.Application.Core;
using Snake.Application.Models;

namespace Snake.Infrastructure.Networking;

public class TcpServer(GameManager manager)
{
    private readonly GameManager _manager = manager;
    private readonly ScreenComposer _screenComposer = new();

    public async Task StartAsync(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"Server started on port {port}!");

        while (true)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                client.NoDelay = true;

                Console.WriteLine("Client connected!");

                _ = HandleClientAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        var buffer = new byte[1024];
        var session = new ClientSession(_manager);

        async Task SendScreen() => await stream.WriteAsync(_screenComposer.Compose(session));

        try
        {
            await SendScreen();

            while (true)
            {
                if (session.Stage != ConnectionStage.Playing)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client disconnected.");
                        break;
                    }

                    var slice = buffer[..bytesRead];
                    var action = InputParser.Parse(slice);
                    session.HandleInput(action, slice);

                    if (session.Stage == ConnectionStage.Playing)
                        _ = RunInputLoop(stream, buffer, session, client);
                }
                else
                {
                    await Task.Delay(200);
                    session.ApplyPendingTick();
                }

                await SendScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client connection error: {ex.Message}");
        }
        finally
        {
            session.EndGame();
            client.Close();
        }
    }

    private static async Task RunInputLoop(NetworkStream stream, byte[] buffer, ClientSession session, TcpClient client)
    {
        try
        {
            while (session.Stage == ConnectionStage.Playing && client.Connected && !session.CancellationToken.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, session.CancellationToken);
                if (bytesRead == 0) break;

                var slice = buffer[..bytesRead];
                var action = InputParser.Parse(slice);
                var dir = InputParser.ToMoveDirection(action);

                if (dir is not null)
                    Interlocked.Exchange(ref session.PendingDirection, dir);
            }
        }
        catch (OperationCanceledException) { }
    }
}
