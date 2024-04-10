namespace ChessLogic.Moves;

public class NormalMove : Move
{
    public override MoveType Type => MoveType.Normal;
    public override Position FromPos { get; }
    public override Position ToPos { get; }

    public NormalMove(Position fromPos, Position toPos) => (FromPos, ToPos) = (fromPos, toPos);
    
    public override void Execute(Board board)
    {
        var piece = board[FromPos];
        board[ToPos] = piece;
        board[FromPos] = null;
        piece.HasMoved = true;
    }
}