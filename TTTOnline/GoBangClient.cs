using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using TTTService;

namespace TTTOnline;

public class GoBangClient
{
    private readonly TcpClient tcpClient;
    private NetworkStream? stream;
    private GoBangService? goBang;
    public int TimeoutMilliSec { get; set; } = 5000;

    public GoBangClient()
    {
        tcpClient = new(AddressFamily.InterNetworkV6);
    }
    internal GoBangClient(BoardMessage board, TcpClient tcpClient)
    {
        goBang = new(board.Rows, board.Cols);
        this.tcpClient = tcpClient;
        stream = tcpClient.GetStream();
    }

    private CancellationToken GetCts()
    {
        CancellationTokenSource tokenSource = new();
        async Task cancel()
        {
            await Task.Delay(TimeoutMilliSec);
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        _ = cancel();
        return tokenSource.Token;
    }

    public async Task<MoveResult> ConnectAsync(string host, int port)
    {
        await tcpClient.ConnectAsync(IPAddress.Parse(host).MapToIPv6(), port, GetCts());
        stream = tcpClient.GetStream();
        var board = await JsonSerializer.DeserializeAsync<BoardMessage>(stream, cancellationToken: GetCts());
        if (board == null) throw new NeverException("Receive null board.");
        goBang = new(board.Rows, board.Cols);
        return await ReceiveMoveAsync();
    }

    private async Task<MoveResult> ReceiveMoveAsync()
    {
        if (goBang == null || stream == null) throw new NeverException("GoBang or stream null when receive.");
        var remoteMove = await JsonSerializer.DeserializeAsync<MoveMessage>(stream);
        if (remoteMove == null) throw new NeverException("Receive null move.");
        try
        {
            var winner = goBang.Judge(remoteMove.X, remoteMove.Y);
            if (winner != GoBangTurnType.Null) tcpClient.Close();
            return new(remoteMove, winner);
        }
        catch (InvalidOperationException)
        {
            throw new CheatException();
        }
    }
    public async Task<MoveResult> MoveAsync(int x, int y)
    {
        if (goBang == null || stream == null) throw new InvalidOperationException("Client not connected.");
        await JsonSerializer.SerializeAsync<MoveMessage>(stream, new(x, y));
        var winner = goBang.Judge(x, y);
        if (winner == GoBangTurnType.Null) return await ReceiveMoveAsync();
        tcpClient.Close();
        return new(x, y, winner);
    }
}
