namespace ChessLogic.Moves;

public abstract class Move
{
    public abstract MoveType Type { get; }
    public abstract Position FromPos { get; }
    public abstract Position ToPos { get; }

    public abstract void Execute(Board board);
}