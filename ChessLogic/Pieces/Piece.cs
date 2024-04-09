namespace ChessLogic.Pieces;

public abstract class Piece
{
    public abstract PieceType Type { get; }

    public abstract Player Color { get; }
    
    public bool HasMoved { get; set; }

    public abstract Piece Copy();
}