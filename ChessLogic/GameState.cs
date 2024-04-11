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

    int _noCaptureOrPawnMoves;

    public GameState(Player player, Board board)
    {
        CurrentPlayer = player;
        Board = board;
    }
    
    public void MakeMove(Move move)
    {
        Board.SetPawnSkipPosition(CurrentPlayer, null);
        bool captureOrPawn = move.Execute(Board);

        _noCaptureOrPawnMoves = captureOrPawn ? 0 : _noCaptureOrPawnMoves + 1;
        
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

        if (FiftyMoveRule())
        {
            Result = new(Player.None, EndReason.FiftyMoveRule);
        }
    }

    public bool IsGameOver() => Result != null;

    bool FiftyMoveRule() => _noCaptureOrPawnMoves / 2 == 50; //по 50 на одного игрока
}