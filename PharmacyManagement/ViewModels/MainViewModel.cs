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
    public RelayCommand OpenNhaCungCapCommand { get; }

    public MainViewModel()
    {
        OpenLoaiThuocCommand = new RelayCommand(_ => CurrentViewModel = new LoaiThuocViewModel(),
            _ => CurrentViewModel is not LoaiThuocViewModel);
        OpenNhaCungCapCommand = new RelayCommand(_ => CurrentViewModel = new NhaCungCapViewModel(),
            _ => CurrentViewModel is not NhaCungCapViewModel);
    }
}
