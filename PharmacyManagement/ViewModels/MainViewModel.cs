namespace PharmacyManagement.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel;

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public RelayCommand NavigateCommand { get; }

    public MainViewModel()
    {
        _currentViewModel = new LoaiThuocViewModel();
        NavigateCommand = new RelayCommand(Navigate, parameter => parameter is "LoaiThuoc");
    }

    private void Navigate(object? parameter)
    {
        switch (parameter)
        {
            case "LoaiThuoc":
                CurrentViewModel = new LoaiThuocViewModel();
                break;
        }
    }
}
