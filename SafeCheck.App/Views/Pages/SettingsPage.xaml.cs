using System.Windows;
using System.Windows.Controls;
using SafeCheck.ViewModels;

namespace SafeCheck.Views.Pages;

public partial class SettingsPage : Page
{
    private readonly Frame _frame;

    public SettingsPage(Frame frame)
    {
        InitializeComponent();
        _frame = frame;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsVM vm)
        {
            VtKeyBox.Password = vm.VtKey;
            GeminiKeyBox.Password = vm.GeminiKey;
            GroqKeyBox.Password = vm.GroqKey;
            CheckPhishKeyBox.Password = vm.CheckPhishKey;
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new HomePage(_frame));

    private void VtKey_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsVM vm)
            vm.VtKey = VtKeyBox.Password;
    }

    private void GeminiKey_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsVM vm)
            vm.GeminiKey = GeminiKeyBox.Password;
    }

    private void GroqKey_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsVM vm)
            vm.GroqKey = GroqKeyBox.Password;
    }

    private void CheckPhishKey_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsVM vm)
            vm.CheckPhishKey = CheckPhishKeyBox.Password;
    }
}
