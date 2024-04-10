using ChessLogic.Moves;

namespace ChessLogic.Pieces;

public class King : Piece
{
    public override PieceType Type => PieceType.King;
    public override Player Color { get; }
    
    static readonly Direction[] Directions = {
        Direction.North,
        Direction.East,
        Direction.South,
        Direction.West,
        Direction.NorthEast,
        Direction.NorthWest,
        Direction.SouthEast,
        Direction.SouthWest,
    };

    public King(Player color) => Color = color;

    public override Piece Copy() => new King(Color)
    {
        HasMoved = HasMoved
    };

    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return MovePositions().Select(to => new NormalMove(from, to));
        
        IEnumerable<Position> MovePositions()
        {
            foreach (var direction in Directions)
            {
                var to = from + direction;

                if (!Board.IsInside(to))
                {
                    continue;
                }

                if (board.IsEmpty(to) || board[to].Color != Color)
                {
                    yield return to;
                }
            }
        }
    }
}