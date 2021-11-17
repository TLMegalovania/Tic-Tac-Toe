using TTTService;

namespace TTTOnline;

public record MoveResult(int X, int Y, GoBangTurnType Winner);
public record GameMessage(int Rows, int Cols, GoBangTurnType YourTurn);
internal record MoveMessage(int X, int Y);
