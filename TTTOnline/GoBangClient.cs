using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using TTTService;

namespace TTTOnline;

public class GoBangClient
{
    private TcpClient? tcpClient;
    private NetworkStream? stream;
    private GoBangService? goBang;

    public GoBangClient() { }

    internal GoBangClient(BoardMessage board, TcpClient tcpClient)
    {
        goBang = new(board.Rows, board.Cols);
        this.tcpClient = tcpClient;
        stream = tcpClient.GetStream();
    }

    public async Task<Task<MoveResult>> ConnectAsync(IPAddress ip, int port, CancellationToken cancellationToken = default)
    {
        tcpClient = new(ip.AddressFamily);
        await tcpClient.ConnectAsync(ip, port, cancellationToken);
        stream = tcpClient.GetStream();
        var board = await JsonSerializer.DeserializeAsync<BoardMessage>(stream, cancellationToken: cancellationToken);
        if (board == null) throw new NeverException("Receive null board.");
        goBang = new(board.Rows, board.Cols);
        return ReceiveMoveAsync();
    }

    private async Task<MoveResult> ReceiveMoveAsync()
    {
        if (goBang == null || stream == null || tcpClient == null) throw new NeverException("GoBang or stream null when receive.");
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
        if (goBang == null || stream == null || tcpClient == null) throw new InvalidOperationException("Client not connected.");
        await JsonSerializer.SerializeAsync<MoveMessage>(stream, new(x, y));
        var winner = goBang.Judge(x, y);
        if (winner == GoBangTurnType.Null) return await ReceiveMoveAsync();
        tcpClient.Close();
        return new(x, y, winner);
    }
}
