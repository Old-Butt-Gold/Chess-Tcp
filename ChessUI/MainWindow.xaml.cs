using System.Windows;
using System.Windows.Input;
using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;

namespace ChessUI;

public partial class MainWindow
{
    ChessClient ChessClient { get; set; }
    ChessViewModel ViewModel { get; }
    
    public MainWindow()
    {
        InitializeComponent();
        List<string> meow = ["1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3"];
        AvailableRoomsListBox.ItemsSource = meow;
        //TcpClient.Connect(ServerIpEndPoint);

        ViewModel = new ChessViewModel
        {
            UiChessManager =
            {
                HighLightGrid = this.HighLightGrid,
                PieceGrid = this.PieceGrid,
                MenuContainer = this.MenuContainer,
                BoardGrid = this.BoardGrid
            },
        };
        DataContext = ViewModel;
    }
    
    void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        ViewModel?.Dispose();
        ChessClient?.Dispose();
    }

    void BotButton_OnClick(object sender, RoutedEventArgs e)
    {
        BotDifficulty botDifficulty = (BotDifficulty)BotDifficultyComboBox.SelectedIndex;
        Player player = (Player)(PlayerComboBox.SelectedIndex + 1);
        
        ViewModel.Start(player, GameType.PlayerVersusBot, botDifficulty);
    }

    void BoardGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        //Из-за того, что Binding присвваивается сразу к null-команде
        if (ViewModel.MouseDownCommand != null && ViewModel.MouseDownCommand.CanExecute(sender))
        {
            ViewModel.MouseDownCommand.Execute(e);
        }
    }
}