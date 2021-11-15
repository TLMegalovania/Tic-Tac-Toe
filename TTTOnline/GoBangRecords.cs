using TTTService;

namespace TTTOnline;

public record MoveResult(int X, int Y, GoBangTurnType Turn)
{
    internal MoveResult(MoveMessage message, GoBangTurnType turn) : this(message.X, message.Y, turn) { }
}
public record BoardMessage(int Rows, int Cols);
internal record MoveMessage(int X, int Y)
{
    public MoveMessage(MoveResult result) : this(result.X, result.Y) { }
}