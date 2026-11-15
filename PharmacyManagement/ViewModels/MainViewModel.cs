namespace PharmacyManagement.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel = new LoaiThuocViewModel();
    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public RelayCommand OpenLoaiThuocCommand { get; }
    public RelayCommand OpenThuocCommand { get; }

    public MainViewModel()
    {
        OpenLoaiThuocCommand = new RelayCommand(_ => CurrentViewModel = new LoaiThuocViewModel(),
            _ => CurrentViewModel is not LoaiThuocViewModel);
        OpenThuocCommand = new RelayCommand(_ => CurrentViewModel = new ThuocViewModel(),
            _ => CurrentViewModel is not ThuocViewModel);
    }
}
