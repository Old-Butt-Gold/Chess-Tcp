using System.IO;
using ChessLogic.Moves;
using ChessLogic.Pieces;

namespace ChessLogic.Bot;

public class BotManager : IDisposable
{
    static readonly string PathToBot = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "stockfish.exe";

    readonly StockfishManager? _stockfishManager;

    public (Move? move, PieceType? pieceType, IEnumerable<Move>? moves) GetBestMove(GameManager gameManager)
    {
        return _stockfishManager!.GetMoveByPosition(gameManager.StateString, gameManager.LegalMovesForPieces);
    }

    public BotManager(BotDifficulty botDifficulty)
    {
        _stockfishManager = new StockfishManager(PathToBot, botDifficulty);
    }

    public void Dispose()
    {
        _stockfishManager?.Dispose();
    }
}