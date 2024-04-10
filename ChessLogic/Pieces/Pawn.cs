using ChessLogic.Moves;

namespace ChessLogic.Pieces;

public class Pawn : Piece
{
    public override PieceType Type => PieceType.Pawn;
    public override Player Color { get; }

    readonly Direction Forward;

    public Pawn(Player color)
    {
        Color = color;
        switch (color)
        {
            case Player.White:
                Forward = Direction.North;
                break;
            case Player.Black:
                Forward = Direction.South;
                break;
        }
    }

    public override Piece Copy() => new Pawn(Color)
    {
        HasMoved = HasMoved
    };

    static bool CanMoveTo(Position pos, Board board) => Board.IsInside(pos) && board.IsEmpty(pos);

    bool CanCaptureAt(Position pos, Board board)
    {
        if (!Board.IsInside(pos) || board.IsEmpty(pos))
        {
            return false;
        }

        return board[pos].Color != Color;
    }

    IEnumerable<Move> ForwardMoves(Position from, Board board)
    {
        Position oneMovePos = from + Forward;

        if (CanMoveTo(oneMovePos, board))
        {
            yield return new NormalMove(from, oneMovePos);

            Position twoMovesPos = oneMovePos + Forward;

            if (!HasMoved && CanMoveTo(twoMovesPos, board))
            {
                yield return new NormalMove(from, twoMovesPos);
            }
        }
    }

    IEnumerable<Move> DiagonalMoves(Position from, Board board)
    {
        foreach (var direction in new[]{Direction.West, Direction.East})
        {
            Position to = from + Forward + direction; //Уже задано North или South в Forward

            if (CanCaptureAt(to, board))
            {
                yield return new NormalMove(from, to);
            }
        }
    }
    
    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return ForwardMoves(from, board).Concat(DiagonalMoves(from, board));
    }
}