namespace ChessLogic.Pieces;

public class Bishop : Piece
{
    public override PieceType Type => PieceType.Bishop;
    public override Player Color { get; }

    public Bishop(Player color) => Color = color;

    public override Piece Copy() => new Bishop(Color)
    {
        HasMoved = HasMoved
    };
}