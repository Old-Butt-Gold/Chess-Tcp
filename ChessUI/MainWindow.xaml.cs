using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;

namespace ChessUI;

public partial class MainWindow
{
    ChessViewModel ViewModel { get; }
    
    //TODO боты ломаются при перевороте поля
    public MainWindow()
    {
        InitializeComponent();
        
        ViewModel = new ChessViewModel(Player.White, GameType.PlayerVersusBot, BotDifficulty.Easy)
        {
            UiChessManager =
            {
                HighLightGrid = this.HighLightGrid,
                PieceGrid = this.PieceGrid,
                MenuContainer = this.MenuContainer,
                BoardGrid = this.BoardGrid
            },
        };
        ViewModel.Start();
        DataContext = ViewModel;

    }

    void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        ViewModel?.Dispose();
    }
}