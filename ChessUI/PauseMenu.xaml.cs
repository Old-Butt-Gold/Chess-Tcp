using System.Windows;
using System.Windows.Controls;

namespace ChessUI;

public partial class PauseMenu : UserControl
{
    public event Action<Option>? OptionSelected;
    
    public PauseMenu()
    {
        InitializeComponent();
    }

    private void Restart_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Continue);
    }

    private void Continue_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Restart);
    }
}