using System.IO;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;
using ChessLogic.Pieces;
using ChessLogic.ResultReasons;

namespace ChessLogic;

public class GameState
{
    static readonly string PathToBot = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "stockfish.exe";
    
    StockfishManager? _stockfishManager;

    public async Task<(Move? move, PieceType? pieceType)> GetBestMoveAsync() => await Task.Run(() => _stockfishManager!.GetMoveByStockFish(_stateString, this));

    public Board Board { get; }
    public Player CurrentPlayer { get; private set; }
    public Result? Result { get; private set; }

    int _noCaptureOrPawnMoves;
    string _stateString;

    readonly Dictionary<string, int> _stateHistory = new();

    public GameState(Player player, Board board)
    {
        CurrentPlayer = player;
        Board = board;

        _stateString = new StateString(CurrentPlayer, board).ToString();
        _stateHistory[_stateString] = 1;
    }

    public GameState(Player player, Board board, BotDifficulty botDifficulty) : this(player, board)
    {
        _stockfishManager = new StockfishManager(PathToBot, botDifficulty);
    }
    
    public void MakeMove(Move move)
    {
        Board.SetPawnSkipPosition(CurrentPlayer, null);
        bool captureOrPawn = move.Execute(Board);

        if (captureOrPawn)
        {
            _noCaptureOrPawnMoves = 0;
            _stateHistory.Clear();
        }
        else
        {
            _noCaptureOrPawnMoves++;
        }

        CurrentPlayer = CurrentPlayer.Opponent();
        UpdateStateString();
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

        if (ThreefoldRepetition())
        {
            Result = new Result(Player.None, EndReason.ThreefoldRepetition);
        }
    }

    public bool IsGameOver() => Result != null;

    bool FiftyMoveRule() => _noCaptureOrPawnMoves / 2 == 50; //по 50 на одного игрока

    void UpdateStateString()
    {
        _stateString = new StateString(CurrentPlayer, Board).ToString();
        
        if (!_stateHistory.TryAdd(_stateString, 1))
        {
            _stateHistory[_stateString]++;
        }
    }

    bool ThreefoldRepetition() => _stateHistory[_stateString] is 3;
}