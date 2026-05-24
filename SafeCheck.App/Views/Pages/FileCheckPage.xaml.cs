using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SafeCheck.ViewModels;

namespace SafeCheck.Views.Pages;

public partial class FileCheckPage : Page
{
    private readonly Frame _frame;

    public FileCheckPage(Frame frame)
    {
        InitializeComponent();
        _frame = frame;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
        => _frame.Navigate(new HomePage(_frame));

    private void ChooseFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Choose a file to scan",
            Filter = "All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() == true && DataContext is FileCheckVM vm)
            vm.FilePath = dlg.FileName;
    }
}
