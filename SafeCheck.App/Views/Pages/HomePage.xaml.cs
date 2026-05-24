using System.Windows;
using System.Windows.Controls;

namespace SafeCheck.Views.Pages;

public partial class HomePage : Page
{
    private readonly Frame _frame;

    public HomePage(Frame frame)
    {
        InitializeComponent();
        _frame = frame;
    }

    private void EmailCheck_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new EmailCheckPage(_frame));

    private void PhoneCheck_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new PhoneCheckPage(_frame));

    private void FileCheck_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new FileCheckPage(_frame));

    private void LinkCheck_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new LinkCheckPage(_frame));

    private void AskAI_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new AskAIPage(_frame));

    private void Settings_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new SettingsPage(_frame));
}
