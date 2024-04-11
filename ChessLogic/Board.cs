using ChessLogic.Pieces;

namespace ChessLogic;

public class Board
{
    readonly Piece[,] _pieces = new Piece[8, 8];

    readonly Dictionary<Player, Position> _pawnSkipPositions = new()
    {
        { Player.White , null},
        { Player.Black , null},
    };
    
    public Position GetPawnSkipPosition(Player player) => _pawnSkipPositions[player];

    public void SetPawnSkipPosition(Player player, Position position) => _pawnSkipPositions[player] = position;

    public Piece this[int row, int col]
    {
        get => _pieces[row, col];
        set => _pieces[row, col] = value;
    }

    public Piece this[Position position]
    {
        get => this[position.Row, position.Column];
        set => this[position.Row, position.Column] = value;
    }
    
    public bool IsEmpty(Position position) => this[position] is null;

    public static Board Initial()
    {
        Board board = new();
        board.AddStartPieces();
        return board;
    }

    void AddStartPieces()
    {
        this[0, 0] = new Rook(Player.Black);
        this[0, 1] = new Knight(Player.Black);
        this[0, 2] = new Bishop(Player.Black);
        this[0, 3] = new Queen(Player.Black);
        this[0, 4] = new King(Player.Black);
        this[0, 5] = new Bishop(Player.Black);
        this[0, 6] = new Knight(Player.Black);
        this[0, 7] = new Rook(Player.Black);
        
        this[7, 0] = new Rook(Player.White);
        this[7, 1] = new Knight(Player.White);
        this[7, 2] = new Bishop(Player.White);
        this[7, 3] = new Queen(Player.White);
        this[7, 4] = new King(Player.White);
        this[7, 5] = new Bishop(Player.White);
        this[7, 6] = new Knight(Player.White);
        this[7, 7] = new Rook(Player.White);

        for (int i = 0; i < 8; i++)
        {
            this[1, i] = new Pawn(Player.Black);
            this[6, i] = new Pawn(Player.White);
        }
    }

    public static bool IsInside(Position position)
    {
        return position.Row is >= 0 and < 8 && position.Column is >= 0 and < 8;
    }

    IEnumerable<Position> PiecePositions()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Position pos = new(i, j);
                if (!IsEmpty(pos))
                {
                    yield return pos;
                }
            }
        }
    }

    public IEnumerable<Position> PiecePositionsFor(Player player)
    {
        return PiecePositions().Where(position => this[position].Color == player);
    }

    public bool IsInCheck(Player player)
    {
        return PiecePositionsFor(player.Opponent()).Any(position => this[position].CanCaptureOpponentKing(position, this));
    }

    public Board Copy()
    {
        Board copy = new();

        foreach (var position in PiecePositions())
        {
            copy[position] = this[position].Copy();
        }

        return copy;
    }

}