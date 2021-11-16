using TTTService;

namespace TTTOnline;

public record MoveResult(int X, int Y, GoBangTurnType Winner)
{
    internal MoveResult(MoveMessage message, GoBangTurnType winner) : this(message.X, message.Y, winner) { }
}
public record BoardMessage(int Rows, int Cols);
internal record MoveMessage(int X, int Y)
{
    public MoveMessage(MoveResult result) : this(result.X, result.Y) { }
}