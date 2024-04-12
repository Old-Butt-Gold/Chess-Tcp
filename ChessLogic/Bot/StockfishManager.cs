using System.Diagnostics;
using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;
using ChessLogic.Pieces;

namespace ChessLogic.Bot;

public class StockfishManager : IDisposable
{
    Process _stockfishProcess = new();

    int _moveTime;
    int _searchDepth;
    
    public StockfishManager(string executablePath, BotDifficulty botDifficulty)
    {
        _stockfishProcess.StartInfo.FileName = executablePath;
        _stockfishProcess.StartInfo.UseShellExecute = false;
        _stockfishProcess.StartInfo.RedirectStandardInput = true;
        _stockfishProcess.StartInfo.RedirectStandardOutput = true;
        _stockfishProcess.StartInfo.CreateNoWindow = true; // Скрыть окно
        _stockfishProcess.Start();

        SetDifficultyLevel(botDifficulty);
    }

    void SetDifficultyLevel(BotDifficulty botDifficulty)
    {
        int skillLevel;
        int threadCount;
        switch (botDifficulty)
        {
            case BotDifficulty.Easy:
                skillLevel = 1;
                threadCount = 1;
                SetSearchDepth(1);
                SetMoveTime(10);
                SetHashSize("16");
                break;
            case BotDifficulty.Medium:
                skillLevel = 5;
                threadCount = 2;
                SetSearchDepth(2);
                SetMoveTime(20);
                SetHashSize("64");
                break;
            case BotDifficulty.Hard:
                skillLevel = 10;
                threadCount = 4;
                SetSearchDepth(3);
                SetMoveTime(30);
                SetHashSize("128");
                break;
            case BotDifficulty.Unreal:
                skillLevel = 20;
                threadCount = 8;
                SetSearchDepth(5);
                SetMoveTime(50);
                SetHashSize("256");
                break;
            default:
                throw new ArgumentException("Invalid difficulty level");
        }
        
        _stockfishProcess.StandardInput.WriteLine("setoption name Skill Level value " + skillLevel);
        _stockfishProcess.StandardInput.WriteLine("setoption name Threads value " + threadCount);
    }
    
    void SetSearchDepth(int searchDepth)
    {
        _searchDepth = searchDepth;
    }

    void SetMoveTime(int moveTime)
    {
        _moveTime = moveTime;
    }

    void SetHashSize(string hashSize)
    {
        _stockfishProcess.StandardInput.WriteLine("setoption name Hash value " + hashSize);
    }
    
    public string GetBestMove(string fenPosition)
    {
        _stockfishProcess.StandardInput.WriteLine("position fen " + fenPosition);
        _stockfishProcess.StandardInput.WriteLine($"go movetime {_moveTime} depth {_searchDepth} ");
        
        _stockfishProcess.StandardInput.Flush();

        string output = _stockfishProcess.StandardOutput.ReadLine();
        while (!output.StartsWith("bestmove"))
        {
            output = _stockfishProcess.StandardOutput.ReadLine();
        }

        string[] outputParts = output.Split(' ');
        return outputParts[1]; // Возвращает лучший ход
    }

    public void Dispose()
    {
        _stockfishProcess.StandardInput.WriteLine("quit");
        _stockfishProcess.WaitForExit();
        _stockfishProcess.Close();
    }
    
    public (Move?, PieceType?) GetMoveByStockFish(string stateString, GameState gameState)
    {
        var str = GetBestMove(stateString);
        var startFile = str[0] - 'a';
        var startRank = 8 - int.Parse(str[1].ToString());
        var endFile = str[2] - 'a';
        var endRank = 8 - int.Parse(str[3].ToString());

        PieceType? pieceType = null; //при превращении пешки
        
        if (str.Length is 5)
        {
            if (char.ToLower(str[4]) == 'q')
                pieceType = PieceType.Queen;
            if (char.ToLower(str[4]) == 'r')
                pieceType = PieceType.Rook;
            if (char.ToLower(str[4]) == 'b')
                pieceType = PieceType.Bishop;
            if (char.ToLower(str[4]) == 'n')
                pieceType = PieceType.Knight;
        }

        var startPos = new Position(startRank, startFile);
            
        IEnumerable<Move> moves = gameState.LegalMovesForPieces(startPos);
        
        Dictionary<Position, Move> movesCache = new();

        Position endPos = new Position(endRank, endFile);
        if (moves.Any())
        {
            foreach (var move in moves)
            {
                movesCache[move.ToPos] = move;
            }

            if (movesCache.TryGetValue(endPos, out var moveFinal))
            {
                return (moveFinal, pieceType);
            }
        }

        return (null, null);
    }
}