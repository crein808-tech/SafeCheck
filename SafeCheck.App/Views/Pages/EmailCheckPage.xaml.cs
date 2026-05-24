using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SafeCheck.ViewModels;

namespace SafeCheck.Views.Pages;

public partial class EmailCheckPage : Page
{
    private readonly Frame _frame;

    public EmailCheckPage(Frame frame)
    {
        InitializeComponent();
        _frame = frame;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new HomePage(_frame));

    private void Input_Loaded(object sender, RoutedEventArgs e)
        => (sender as TextBox)?.Focus();

    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is EmailCheckVM vm && vm.ScanCommand.CanExecute(null))
        {
            vm.ScanCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void Paste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText())
            InputBox.Text = Clipboard.GetText().Trim();
    }
}
