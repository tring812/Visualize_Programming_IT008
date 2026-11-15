using PharmacyManagement.ViewModels;

namespace PharmacyManagement.Models;

public class KhachHang : BaseViewModel
{
    public int MaKH { get; set; }

    private string _hoTen = "";
    public string HoTen { get => _hoTen; set => SetProperty(ref _hoTen, value); }

    private string _diaChi = "";
    public string DiaChi { get => _diaChi; set => SetProperty(ref _diaChi, value); }

    private string _soDienThoai = "";
    public string SoDienThoai { get => _soDienThoai; set => SetProperty(ref _soDienThoai, value); }
}
