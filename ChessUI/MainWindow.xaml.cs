using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;

namespace ChessUI;

public partial class MainWindow
{
    private GameType _gameType;
    private BotDifficulty _botDifficulty;
    private Player _startPlayer;
    
    //TODO боты ломаются при перевороте поля
    public MainWindow()
    {
        InitializeComponent();
        _gameType = GameType.BotVersusBot;
        _botDifficulty = BotDifficulty.Easy;
        _startPlayer = Player.White;
        
        InitializeComponent();
        var viewModel = new MainWindowViewModel(_startPlayer, _gameType, _botDifficulty)
        {
            HighLightGrid = this.HighLightGrid,
            PieceGrid = this.PieceGrid,
            MenuContainer = this.MenuContainer,
            BoardGrid = this.BoardGrid,
        };
        viewModel.Start();
        DataContext = viewModel;
    }
}

