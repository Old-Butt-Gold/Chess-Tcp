namespace ChessLogic.ResultReasons;

public class Result
{
    public Player Winner { get; }
    public EndReason Reason { get; }

    public Result(Player winner, EndReason reason) => (Winner, Reason) = (winner, reason);

    public static Result Win(Player winner) => new Result(winner, EndReason.Checkmate);
    
    public static Result Draw(EndReason reason) => new Result(Player.None, reason);
}