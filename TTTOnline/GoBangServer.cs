using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace TTTOnline;
public class GoBangServer
{
    public TcpListener Listener { get; }
    public BoardMessage Board { get; set; }
    public GoBangServer()
    {
        Listener = new(IPAddress.IPv6Any, 0);
        Board = new(5, 5);
    }

    public async Task<GoBangClient> GetClientAsync(CancellationToken cancellationToken = default)
    {
        Listener.Start();
        try
        {
            var client = await Listener.AcceptTcpClientAsync(cancellationToken);
            await JsonSerializer.SerializeAsync(client.GetStream(), Board, cancellationToken: cancellationToken);
            return new(Board, client);
        }
        finally
        {
            Listener.Stop();
        }
    }
}
