namespace ChessLogic.Moves;

public class CastleMove : Move
{
    public override MoveType Type { get; }
    public override Position FromPos { get; }
    public override Position ToPos { get; }

    readonly Direction KingMoveDirection;
    readonly Position RookFromPos;
    readonly Position RookToPos;

    public CastleMove(MoveType type, Position kingPos)
    {
        Type = type;
        FromPos = kingPos;

        switch (type)
        {
            case MoveType.CastleKS:
                KingMoveDirection = Direction.East;
                ToPos = new Position(kingPos.Row, 6);
                RookFromPos = new Position(kingPos.Row, 7);
                RookToPos = new Position(kingPos.Row, 5);
                break;
            case MoveType.CastleQS:
                KingMoveDirection = Direction.West;
                ToPos = new Position(kingPos.Row, 2);
                RookFromPos = new Position(kingPos.Row, 0);
                RookToPos = new Position(kingPos.Row, 3);
                break;
        }
    }
    
    public override void Execute(Board board)
    {
        new NormalMove(FromPos, ToPos).Execute(board);
        new NormalMove(RookFromPos, RookToPos).Execute(board);
    }

    public override bool IsLegal(Board board)
    {
        Player player = board[FromPos].Color;

        if (board.IsInCheck(player))
        {
            return false;
        }

        //Для проверки на шах, во время рокировки короля
        Board copy = board.Copy();
        Position kingPositionInCopy = FromPos;

        for (int i = 0; i < 2; i++)
        {
            new NormalMove(kingPositionInCopy, kingPositionInCopy + KingMoveDirection).Execute(copy);
            kingPositionInCopy += KingMoveDirection;

            if (copy.IsInCheck(player))
            {
                return false;
            }
        }

        return true;
    }
}