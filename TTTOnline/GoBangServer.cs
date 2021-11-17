using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using TTTService;

namespace TTTOnline;
public class GoBangServer
{
    public TcpListener Listener6 { get; }
    public TcpListener Listener4 { get; }
    public GameMessage GameInfo { get; set; }

    private GoBangTurnType _mySide;

    public GoBangTurnType MySide
    {
        get => _mySide;
        set
        {
            _mySide = value switch
            {
                GoBangTurnType.Black or GoBangTurnType.White => _mySide,
                _ => throw new InvalidOperationException("Invalid side")
            };
        }
    }

    public GoBangServer(GoBangTurnType mySide = GoBangTurnType.Black, int rows = 5, int cols = 5)
    {
        if (rows <= 0) throw new ArgumentException("Not positive.", nameof(rows));
        if (cols <= 0) throw new ArgumentException("Not positive.", nameof(cols));
        Listener6 = new(IPAddress.IPv6Any, 0);
        Listener4 = new(IPAddress.Any, 0);
        MySide = mySide;
        GameInfo = new(rows, cols, mySide);
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
            var stream = client.GetStream();
            await JsonSerializer.SerializeAsync(stream, GameInfo with { YourTurn = GameInfo.YourTurn.Opposite() }, cancellationToken: cancellationToken);
            return new(GameInfo, client);
        }
        finally
        {
            Listener4.Stop();
            Listener6.Stop();
        }
    }
}
