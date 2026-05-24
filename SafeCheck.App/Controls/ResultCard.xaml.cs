using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SafeCheck.Models;

namespace SafeCheck.Controls;

public partial class ResultCard : UserControl
{
    public static readonly DependencyProperty ResultProperty =
        DependencyProperty.Register(nameof(Result), typeof(ScanResult), typeof(ResultCard));

    public ScanResult? Result
    {
        get => (ScanResult?)GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public ResultCard()
    {
        InitializeComponent();
    }

    private static void OpenUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == "https" || uri.Scheme == "http"))
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
    }

    private void SearchGoogle_Click(object sender, RoutedEventArgs e)
    {
        if (Result?.GoogleSearchUrl is { } url)
            OpenUrl(url);
    }

    private void LookupLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url)
            OpenUrl(url);
    }
}
