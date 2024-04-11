using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;
using ChessLogic.Pieces;
using ChessLogic.ResultReasons;

namespace ChessLogic;

public class GameState
{
    public Board Board { get; }
    public Player CurrentPlayer { get; private set; }
    public Result? Result { get; private set; }

    public GameState(Player player, Board board)
    {
        CurrentPlayer = player;
        Board = board;
    }
    
    public void MakeMove(Move move)
    {
        Board.SetPawnSkipPosition(CurrentPlayer, null);
        move.Execute(Board);
        CurrentPlayer = CurrentPlayer.Opponent();
        CheckForGameOver();
    }

    public IEnumerable<Move> LegalMovesForPieces(Position position)
    {
        if (Board.IsEmpty(position) || Board[position].Color != CurrentPlayer)
        {
            return Enumerable.Empty<Move>();
        }
        
        Piece piece = Board[position];
        IEnumerable<Move> moveCandidates = piece.GetMoves(position, Board);
        return moveCandidates.Where(move => move.IsLegal(Board));
    }

    IEnumerable<Move> AllLegalMovesFor(Player player)
    {
        var moveCandidates = Board.PiecePositionsFor(player).SelectMany(position => Board[position].GetMoves(position, Board));

        return moveCandidates.Where(move => move.IsLegal(Board));
    }

    void CheckForGameOver()
    {
        if (!AllLegalMovesFor(CurrentPlayer).Any())
        {
            Result = Board.IsInCheck(CurrentPlayer) 
                ? new(CurrentPlayer.Opponent(), EndReason.Checkmate) 
                : new(Player.None, EndReason.Stalemate);
        }

        if (Board.InsufficientMaterial())
        {
            Result = new Result(CurrentPlayer, EndReason.InsufficientMaterial);
        }
    }

    public bool IsGameOver() => Result != null;
}