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
    private readonly byte[] _buffer;
    private GoBangTurnType _myTurn, _thisTurn;

    public GoBangClient()
    {
        _buffer = new byte[1024];
        _thisTurn = GoBangTurnType.Black;
    }

    internal GoBangClient(GameMessage board, TcpClient tcpClient) : this()
    {
        goBang = new(board.Rows, board.Cols);
        _myTurn = board.YourTurn;
        this.tcpClient = tcpClient;
        stream = tcpClient.GetStream();
    }

    public async Task<GameMessage> ConnectAsync(IPAddress ip, int port, CancellationToken cancellationToken = default)
    {
        tcpClient?.Close();
        tcpClient = new(ip.AddressFamily);
        try
        {
            await tcpClient.ConnectAsync(ip, port, cancellationToken);
            stream = tcpClient.GetStream();
            int size = await stream.ReadAsync(_buffer, cancellationToken);
            if (size == 0)
            {
                tcpClient.Close();
                throw new SocketException(995);
            }
            var board = JsonSerializer.Deserialize<GameMessage>(new ReadOnlySpan<byte>(_buffer, 0, size));
            if (board == null) throw new NeverException("Receive null board.");
            goBang = new(board.Rows, board.Cols);
            _myTurn = board.YourTurn;
            return board;
        }
        catch (Exception)
        {
            tcpClient.Close();
            throw;
        }
    }

    public async Task<MoveResult?> ReceiveMoveAsync()
    {
        if (goBang == null || stream == null || tcpClient == null) throw new InvalidOperationException("GoBang or stream null when receive.");
        if (_thisTurn == _myTurn) return null;
        int size = await stream.ReadAsync(_buffer);
        if(size == 0)
        {
            tcpClient.Close();
            throw new SocketException(995);
        }
        var remoteMove = JsonSerializer.Deserialize<MoveMessage>(new ReadOnlySpan<byte>(_buffer, 0, size));
        if (remoteMove == null) throw new NeverException("Receive null move.");
        var winner = goBang.Judge(remoteMove.X, remoteMove.Y);
        _thisTurn = _thisTurn.Opposite();
        if (winner != GoBangTurnType.Null) tcpClient.Close();
        return new(remoteMove.X, remoteMove.Y, winner);
    }
    public async Task<MoveResult?> MoveAsync(int x, int y)
    {
        if (goBang == null || stream == null || tcpClient == null) throw new InvalidOperationException("Client not connected.");
        if (_thisTurn != _myTurn) return null;
        try
        {
            var winner = goBang.Judge(x, y);
            _thisTurn = _thisTurn.Opposite();
            await JsonSerializer.SerializeAsync<MoveMessage>(stream, new(x, y));
            if (winner == GoBangTurnType.Null) return await ReceiveMoveAsync();
            tcpClient.Close();
            return new(x, y, winner);
        }
        catch (CheatException)
        {
            return null;
        }
    }
}
