using System.Net;
using System.Net.Sockets;
using System.Text;
using Snake.Application.Core;
using Snake.Application.Models;

namespace Snake.Application;

// inspired by https://medium.com/@jm.keuleyan/c-tcp-communications-building-a-client-server-chat-a2155d585191
// made async because learn.microsoft.com says so

// TODO: better input reading (currently buggy due to lack of move queueing)
public class TcpServer(GameManager manager)
{
    private readonly GameManager _manager = manager;

    private enum ConnectionStage
    {
        EnteringName,
        ConfiguringGridSize,
        Playing,
        GameEnded
    }

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
                client.NoDelay = true; // in manual testing, this is found to make the game more responsive

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
        var stage = ConnectionStage.EnteringName;

        // Game configuration
        string username = string.Empty;
        uint gridSize = 8;
        GameConfig? gameConfig = null;

        // Game state
        GameInstance? gameInstance = null;
        object? pendingDirection = null;

        async Task sendByteArray(byte[] bytes) => await stream.WriteAsync(bytes, 0, bytes.Length);
        async Task sendString(string s) => await sendByteArray(Encoding.UTF8.GetBytes(s));

        CancellationTokenSource? cancellation = null;

        async Task EndGame()
        {
            if (gameInstance is null) return;
            cancellation?.Cancel();
            _ = Interlocked.Exchange(ref pendingDirection, null);
            _manager.EndGame(gameInstance.Id);
            stage = ConnectionStage.GameEnded;
        }

        async Task StartNewGame()
        {
            if (gameConfig is null) return;
            await EndGame();

            gameInstance = _manager.StartNewGame((GameConfig)gameConfig);
            gameInstance.GameOver += async (e) => { await EndGame(); };

            cancellation = new CancellationTokenSource();

            stage = ConnectionStage.Playing;

            // Movement thread (WASD/arrow keys)
            _ = Task.Run(async () =>
            {
                try
                {
                    while (stage == ConnectionStage.Playing && client.Connected && !cancellation.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellation.Token);
                        if (bytesRead == 0) break; // client disconnected

                        var slice = buffer[..bytesRead];

                        MoveDirection? dir = slice switch
                        {
                            [(byte)'W' or (byte)'w'] or [0x1B, 0x5B, 0x41] => MoveDirection.Up,
                            [(byte)'S' or (byte)'s'] or [0x1B, 0x5B, 0x42] => MoveDirection.Down,
                            [(byte)'A' or (byte)'a'] or [0x1B, 0x5B, 0x44] => MoveDirection.Left,
                            [(byte)'D' or (byte)'d'] or [0x1B, 0x5B, 0x43] => MoveDirection.Right,
                            _ => null
                        };

                        if (dir is not null)
                        {
                            Interlocked.Exchange(ref pendingDirection, dir);
                        }
                    }
                }
                catch (OperationCanceledException) { }
            });
        }

        async Task SendScreen()
        {
            await sendByteArray(TcpMagicBytes.CLEAR_SCREEN_AND_RESET_CURSOR);
            switch (stage)
            {
                case ConnectionStage.EnteringName:
                    await sendByteArray(TcpMagicBytes.SNAKE_ART);
                    await sendString($"\r\nPlease enter your name: {username}\r\nPress enter to confirm");
                    break;
                case ConnectionStage.ConfiguringGridSize:
                    await sendByteArray(TcpMagicBytes.SNAKE_ART);
                    await sendString($"\r\nThe current grid size is {gridSize}\r\nUse W and S keys to adjust the size\r\nPress enter to start the game");
                    break;
                case ConnectionStage.Playing when gameInstance is not null:
                    await sendString(gameInstance.Render() + $"\r\nScore: {gameInstance.Score}\r\nPress WASD or arrow keys to move");
                    break;
                case ConnectionStage.GameEnded when gameInstance is not null:
                    await sendString($"Game over!\r\nScore: {gameInstance.Score}\r\nPress 1 to replay with the same settings\r\nPress 2 to go back to configuration screen");
                    break;
            }
        }

        try
        {
            await SendScreen();

            while (true)
            {
                if (stage != ConnectionStage.Playing)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client disconnected.");
                        break;
                    }

                    var slice = buffer[..bytesRead];

                    switch (stage)
                    {
                        case ConnectionStage.EnteringName:
                            // Backspace
                            if (slice is [0x7F])
                            {
                                username = string.IsNullOrEmpty(username) ? username : username[..^1];
                            }
                            // Enter
                            else if (slice is [0x0D])
                            {
                                if (GameConfig.IsValidUserName(username))
                                {
                                    stage = ConnectionStage.ConfiguringGridSize;
                                }
                            }
                            // Letter
                            else if (Ascii.IsValid(slice))
                            {
                                string newUsername = username + Encoding.UTF8.GetString(slice);
                                if (GameConfig.IsValidUserName(newUsername))
                                {
                                    username = newUsername;
                                }
                            }
                            break;
                        case ConnectionStage.ConfiguringGridSize:
                            // W
                            if (slice is [0x57] or [0x77])
                            {
                                if (GameConfig.IsValidGridSize(gridSize + 1))
                                {
                                    ++gridSize;
                                }
                            }
                            // S
                            else if (slice is [0x53] or [0x73])
                            {
                                if (GameConfig.IsValidGridSize(gridSize - 1))
                                {
                                    --gridSize;
                                }
                            }
                            // Enter
                            else if (slice is [0x0D])
                            {
                                gameConfig = new GameConfig(
                                    gridSize: gridSize,
                                    userName: username
                                );
                                await StartNewGame();
                            }
                            break;
                        case ConnectionStage.GameEnded:
                            if (slice is [0x31])
                            {
                                await StartNewGame();
                            }
                            else if (slice is [0x32])
                            {
                                stage = ConnectionStage.EnteringName;
                            }
                            break;
                    }
                }
                else if (gameInstance is not null)
                {
                    await Task.Delay(200);
                    gameInstance.Tick((MoveDirection?)Interlocked.Exchange(ref pendingDirection, null));
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
            await EndGame();
            client.Close();
        }
    }
}
