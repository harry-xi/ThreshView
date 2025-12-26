using Avalonia.Controls;
using ThreshView.ViewModels;

namespace ThreshView.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}