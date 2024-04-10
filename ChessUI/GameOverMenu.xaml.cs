using System.Windows;
using System.Windows.Controls;
using ChessLogic;
using ChessLogic.ResultReasons;

namespace ChessUI;

public partial class GameOverMenu
{
    public event Action<Option>? OptionSelected;
    
    public GameOverMenu(GameState gameState)
    {
        InitializeComponent();

        var result = gameState.Result;

        WinnerText.Text = GetWinnerText(result!.Winner);
        ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
    }
    
    static string GetWinnerText(Player winner) =>
        winner switch
        {
            Player.White => "WHITE WINS",
            Player.Black => "BLACK WINS",
            _ => "DRAW",
        };

    static string PlayerString(Player player) =>
        player switch
        {
            Player.White => "WHITE",
            Player.Black => "BLACK",
            _ => string.Empty,
        };
    
    static string GetReasonText(EndReason reason, Player currentPlayer)
    {
        return reason switch
        {
            EndReason.Stalemate => $"STALEMATE – {PlayerString(currentPlayer)} CAN'T MOVE",
            EndReason.Checkmate => $"CHECKMATE – {PlayerString(currentPlayer)} CAN'T MOVE",
            EndReason.FiftyMoveRule => $"FIFTY-MOVE RULE",
            EndReason.InsufficientMaterial => $"INSUFFICIENT MATERIAL",
            EndReason.ThreefoldRepetition => $"THREEFOLD REPETITION",
            _ => string.Empty,
        };
    }
    
    private void Exit_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Exit);
    }

    private void Restart_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Restart);
    }
}