using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace TTTOnline;
public class GoBangServer
{
    public TcpListener Listener6 { get; }
    public TcpListener Listener4 { get; }
    public BoardMessage Board { get; set; }
    public GoBangServer(int rows = 5, int cols = 5)
    {
        if (rows <= 0) throw new ArgumentException("Not positive.", nameof(rows));
        if (cols <= 0) throw new ArgumentException("Not positive.", nameof(cols));
        Listener6 = new(IPAddress.IPv6Any, 0);
        Listener4 = new(IPAddress.Any, 0);
        Board = new(rows, cols);
    }

    public async Task<GoBangClient> GetClientAsync(CancellationToken cancellationToken = default)
    {
        Listener4.Start();
        Listener6.Start();
        try
        {
            var tasks = new Task<TcpClient>[2]
            {
                Listener6.AcceptTcpClientAsync(cancellationToken).AsTask(),
                Listener4.AcceptTcpClientAsync(cancellationToken).AsTask()
            };
            var anyTask = await Task.WhenAny(tasks);
            var client = anyTask.Result;
            await JsonSerializer.SerializeAsync(client.GetStream(), Board, cancellationToken: cancellationToken);
            return new(Board, client);
        }
        finally
        {
            Listener4.Stop();
            Listener6.Stop();
        }
    }
}
