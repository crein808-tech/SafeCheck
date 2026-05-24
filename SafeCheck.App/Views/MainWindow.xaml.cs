using System.Windows;
using SafeCheck.Views.Pages;

namespace SafeCheck.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new HomePage(MainFrame));
    }
}
