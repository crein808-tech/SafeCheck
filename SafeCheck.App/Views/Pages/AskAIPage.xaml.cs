using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SafeCheck.ViewModels;

namespace SafeCheck.Views.Pages;

public partial class AskAIPage : Page
{
    private readonly Frame _frame;

    public AskAIPage(Frame frame)
    {
        InitializeComponent();
        _frame = frame;

        if (DataContext is AskAIVM vm)
            vm.Messages.CollectionChanged += Messages_CollectionChanged;
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ChatScroll.ScrollToEnd();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new HomePage(_frame));

    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is AskAIVM vm && vm.CanAsk)
            vm.AskCommand.Execute(null);
    }
}
