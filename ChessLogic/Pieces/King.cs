namespace ChessLogic.Pieces;

public class King : Piece
{
    public override PieceType Type => PieceType.King;
    public override Player Color { get; }

    public King(Player color) => Color = color;

    public override Piece Copy() => new King(Color)
    {
        HasMoved = HasMoved
    };
}