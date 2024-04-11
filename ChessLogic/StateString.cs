using System.Text;
using ChessLogic.CoordinateClasses;
using ChessLogic.Pieces;

namespace ChessLogic;

public class StateString
{
    readonly StringBuilder sb = new();

    public StateString(Player currentPlayer, Board board) //FEN
    {
        AddPiecePlacement(board);
        sb.Append(' ');
        AddCurrentPlayer(currentPlayer);
        sb.Append(' ');
        AddCastlingRights(board);
        sb.Append(' ');
        AddEnPassant(board, currentPlayer);
    }

    public override string ToString() => sb.ToString();

    void AddPiecePlacement(Board board)
    {
        for (int i = 0; i < 8; i++)
        {
            if (i != 0)
            {
                sb.Append('/');
            }
            
            AddRowData(board, i);
        }
        
        void AddRowData(Board board, int row)
        {
            int empty = 0;

            for (int i = 0; i < 8; i++)
            {
                if (board[row, i] is null)
                {
                    empty++;
                }
                else
                {

                    if (empty > 0)
                    {
                        sb.Append(empty);
                        empty = 0;
                    }

                    sb.Append(PieceChar(board[row, i]));
                }
            }

            if (empty > 0)
            {
                sb.Append(empty);
            }
        
            char PieceChar(Piece piece)
            {
                char c = piece.Type switch
                {
                    PieceType.Pawn => 'p',
                    PieceType.Knight => 'n',
                    PieceType.Rook => 'r',
                    PieceType.Bishop => 'b',
                    PieceType.Queen => 'q',
                    PieceType.King => 'k',
                    _ => ' ',
                };

                return piece.Color is Player.White ? char.ToUpper(c) : c;
            }
        }
    }

    void AddCurrentPlayer(Player currentPlayer) => sb.Append(currentPlayer == Player.White ? 'w' : 'b');

    void AddCastlingRights(Board board)
    {
        bool castleWks = board.CastleRightKs(Player.White);
        bool castleBks = board.CastleRightKs(Player.Black);
        bool castleWqs = board.CastleRightQs(Player.White);
        bool castleBqs = board.CastleRightQs(Player.Black);

        if (!(castleWqs || castleWks || castleBqs || castleBks))
        {
            sb.Append('-');
            return;
        }

        if (castleWks)
        {
            sb.Append('K');
        }

        if (castleWqs)
        {
            sb.Append('Q');
        }

        if (castleBks)
        {
            sb.Append('k');
        }

        if (castleBqs)
        {
            sb.Append('q');
        }
    }

    void AddEnPassant(Board board, Player currentPlayer)
    {
        if (!board.CanCaptureEnPassant(currentPlayer))
        {
            sb.Append('-');
            return;
        }

        Position position = board.GetPawnSkipPosition(currentPlayer.Opponent());
        char file = (char)('a' + position.Column);
        int rank = 8 - position.Row;

        sb.Append(file);
        sb.Append(rank);
    }
}