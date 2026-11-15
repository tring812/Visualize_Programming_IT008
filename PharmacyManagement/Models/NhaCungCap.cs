using PharmacyManagement.ViewModels;

namespace PharmacyManagement.Models;

public class NhaCungCap : BaseViewModel
{
    public int MaNCC { get; set; }

    private string _tenNCC = "";
    public string TenNCC { get => _tenNCC; set => SetProperty(ref _tenNCC, value); }

    private string _diaChi = "";
    public string DiaChi { get => _diaChi; set => SetProperty(ref _diaChi, value); }

    private string _soDienThoai = "";
    public string SoDienThoai { get => _soDienThoai; set => SetProperty(ref _soDienThoai, value); }
}
