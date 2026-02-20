using System.Net;
using System.Net.Sockets;
using System.Text;
using Snake.Application;
using Snake.Application.Core;
using Snake.Tests.Helpers;

namespace Snake.Tests;

public class TcpTests
{
    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public async Task StartAsync_AcceptsClientConnection()
    {
        int port = GetAvailablePort();
        var manager = new GameManager(new TestDataRepository());
        var server = new TcpServer(manager);

        var serverTask = Task.Run(() => server.StartAsync(port));

        await Task.Delay(100);

        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        Assert.True(client.Connected);
        client.Close();
    }

    [Fact]
    public async Task ClientConnection_ReceivesInitialScreen()
    {
        int port = GetAvailablePort();
        var manager = new GameManager(new TestDataRepository());
        var server = new TcpServer(manager);

        var serverTask = Task.Run(() => server.StartAsync(port));

        await Task.Delay(100);

        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        using var stream = client.GetStream();
        var buffer = new byte[2048];

        await Task.Delay(100);

        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Assert.True(client.Connected);
        Assert.Contains("Please enter your name", response);
        client.Close();
    }
}
