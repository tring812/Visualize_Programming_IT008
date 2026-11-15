using PharmacyManagement.ViewModels;

namespace PharmacyManagement.Models;

// Một dòng của bảng LoaiThuoc. Kế thừa BaseViewModel để khi đổi TenLoai,
// dòng tương ứng trên DataGrid tự cập nhật.
public class LoaiThuoc : BaseViewModel
{
    private int _maLoai;
    private string _tenLoai = "";

    public int MaLoai { get => _maLoai; set => SetProperty(ref _maLoai, value); }

    public string TenLoai { get => _tenLoai; set => SetProperty(ref _tenLoai, value); }
}
