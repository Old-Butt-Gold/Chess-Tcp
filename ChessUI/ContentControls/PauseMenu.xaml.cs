using System.Windows;
using System.Windows.Controls;

namespace ChessUI.ContentControls;

public partial class PauseMenu
{
    public event Action<Option>? OptionSelected;
    
    public PauseMenu()
    {
        InitializeComponent();
    }

    private void Restart_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Restart);
    }

    private void Continue_OnClick(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Continue);
    }
}