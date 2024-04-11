using ChessLogic.CoordinateClasses;
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

    public static bool IsInside(Position position) => position.Row is >= 0 and < 8 && position.Column is >= 0 and < 8;

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

    Counting CountPieces()
    {
        Counting counting = new();
        foreach (var position in PiecePositions())
        {
            Piece piece = this[position];
            counting.Increment(piece.Color, piece.Type);
        }

        return counting;
    }

    public bool InsufficientMaterial()
    {
        Counting counting = CountPieces();

        return IsKingVersusKing(counting) || IsKingBishopVersusKing(counting)
                                          || IsKingKnightVersusKing(counting) || IsKingBishopVersusKingBishop(counting)
                                          || IsKingSolo(counting) || IsKingVersusTwoKnightsKing(counting) ||
                                          IsKingBishopVersusKingKnight(counting);
    }

    bool IsKingVersusKing(Counting counting) => counting.TotalCount is 2;

    bool IsKingBishopVersusKing(Counting counting) => counting.TotalCount is 3 && (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);

    bool IsKingKnightVersusKing(Counting counting) => counting.TotalCount == 3 && (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
    
    bool IsKingBishopVersusKingBishop(Counting counting)
    {
        if (counting.TotalCount != 4)
        {
            return false;
        }

        if (counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1)
        {
            return false;
        }

        Position whiteBishopPos = FindPiece(Player.White, PieceType.Bishop);
        Position blackBishopPos = FindPiece(Player.Black, PieceType.Bishop);

        return whiteBishopPos.SquareColor() == blackBishopPos.SquareColor();
    }

    bool IsKingSolo(Counting counting) => counting.TotalCount is 17 && 
        ((counting.TotalWhite() is 16 && counting.Black(PieceType.King) is 1) ||
        (counting.TotalBlack() is 16 && counting.White(PieceType.King) is 1));

    bool IsKingVersusTwoKnightsKing(Counting counting) => counting.TotalCount is 4 && (counting.White(PieceType.Knight) == 2 || counting.Black(PieceType.Knight) == 2);

    bool IsKingBishopVersusKingKnight(Counting counting) => counting.TotalCount is 4 &&
                                                            ((counting.White(PieceType.Knight) == 1 && counting.Black(PieceType.Bishop) == 1) ||
                                                             (counting.Black(PieceType.Knight) == 1 && counting.White(PieceType.Bishop) == 1));
    
    Position FindPiece(Player color, PieceType type) => PiecePositionsFor(color).First(position => this[position].Type == type);
}