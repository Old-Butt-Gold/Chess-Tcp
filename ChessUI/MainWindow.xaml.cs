using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;

namespace ChessUI;

public partial class MainWindow
{
    ChessClient ChessClient { get; set; } = new();
    ChessViewModel ViewModel { get; }
    
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
            ViewModel.MouseDownCommand?.Execute(e);
        }
    }

    async void Connect_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IPAddress.TryParse(ServerIpTextBox.Text, out var ipAddress))
        {
            MessageBox.Show("Invalid IP address.");
            return;
        }

        if (!int.TryParse(PortTextBox.Text, out var port))
        {
            MessageBox.Show("Invalid port number.");
            return;
        }

        var button = e.Source as Button;
        button.IsEnabled = false;
        try
        {
            await ChessClient.ConnectAsync(ipAddress, port);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    async void Disconnect_OnClick(object sender, RoutedEventArgs e)
    {
        await ChessClient.DisconnectAsync();
    }

    async void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ChessClient.IsConnected)
        {
            MessageBox.Show("You are not connected to the server.");
            return;
        }
        
        var button = e.Source as Button;
        button!.IsEnabled = false;
        await ChessClient.SendMessageAsync("show_rooms");
        var message = await ChessClient.ReceiveMessageAsync();

        var rooms = message?.Split(ChessClient.MessageRegex, StringSplitOptions.RemoveEmptyEntries);

        List<RoomInfo> roomInfos = [];

        foreach (var room in rooms)
        {
            var components = room.Split(":", StringSplitOptions.RemoveEmptyEntries);
            roomInfos.Add(new RoomInfo(components[0], int.Parse(components[1])));
        }

        AvailableRoomsListBox.ItemsSource = roomInfos;
        button.IsEnabled = true;
    }
    
    class RoomInfo
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public RoomInfo(string name, int count) => (Name, Count) = (name, count);
    }
}
