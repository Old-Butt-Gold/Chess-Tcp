using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;

namespace ChessUI;

public partial class MainWindow
{
    ChessViewModel ViewModel { get; }

    TcpClient TcpClient { get; } = new();

    IPEndPoint ServerIpEndPoint { get; } = IPEndPoint.Parse("127.0.0.1:5555");
    
    public MainWindow()
    {
        InitializeComponent();
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
        TcpClient?.Dispose();
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