using System.Windows.Controls;

namespace SafeCheck.ViewModels;

public class MainWindowVM : BaseViewModel
{
    private Page? _currentPage;
    public Page? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }
}
