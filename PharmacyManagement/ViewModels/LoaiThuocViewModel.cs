using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Data;
using PharmacyManagement.Models;

namespace PharmacyManagement.ViewModels;

// ViewModel của màn Loại thuốc: giữ danh sách, dòng đang chọn, dữ liệu form và các lệnh.
// Không gọi MessageBox, không đụng control: mọi thông báo đi qua ErrorMessage.
public class LoaiThuocViewModel : BaseViewModel
{
    private const int TenLoaiToiDa = 100;   // khớp NVARCHAR(100) trong 01_schema.sql

    private readonly LoaiThuocRepository _repo = new();

    // ObservableCollection báo cho DataGrid mỗi khi thêm/xóa phần tử.
    public ObservableCollection<LoaiThuoc> Items { get; } = new();

    private LoaiThuoc? _selectedItem;
    public LoaiThuoc? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value) && value != null)
            {
                TenLoai = value.TenLoai;   // đổ dòng đang chọn xuống form để sửa
                ErrorMessage = "";
            }
        }
    }

    // Thuộc tính của form nhập, bind với TextBox "Tên loại".
    private string _tenLoai = "";
    public string TenLoai { get => _tenLoai; set => SetProperty(ref _tenLoai, value); }

    // Bind với TextBlock màu đỏ; chuỗi rỗng là không có lỗi.
    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    public RelayCommand AddCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public LoaiThuocViewModel()
    {
        AddCommand = new RelayCommand(_ => ClearForm());
        SaveCommand = new RelayCommand(_ => Save(), _ => IsValid());
        DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);
        Load();
    }

    private void Load()
    {
        try
        {
            Items.Clear();
            foreach (var x in _repo.GetAll())
                Items.Add(x);
            ErrorMessage = "";
        }
        catch (SqlException)
        {
            ErrorMessage = "Không kết nối được CSDL. Kiểm tra SQL Server đã chạy và đã tạo PharmacyDB chưa.";
        }
    }

    // CanExecute của nút Lưu: tên trống thì nút tự mờ đi.
    private bool IsValid() => !string.IsNullOrWhiteSpace(TenLoai);

    private void Save()
    {
        var ten = TenLoai.Trim();
        if (ten.Length > TenLoaiToiDa)
        {
            ErrorMessage = $"Tên loại tối đa {TenLoaiToiDa} ký tự.";
            return;
        }

        // Bảng LoaiThuoc không có ràng buộc UNIQUE trên TenLoai, nên kiểm tra trùng ở đây.
        // Bỏ qua chính dòng đang sửa để sửa mà giữ nguyên tên vẫn lưu được.
        bool trungTen = Items.Any(x => x != SelectedItem
            && string.Equals(x.TenLoai, ten, StringComparison.CurrentCultureIgnoreCase));
        if (trungTen)
        {
            ErrorMessage = "Tên loại này đã tồn tại.";
            return;
        }

        try
        {
            if (SelectedItem == null)
            {
                var item = new LoaiThuoc { TenLoai = ten };
                item.MaLoai = _repo.Insert(item);
                Items.Add(item);
            }
            else
            {
                // Ghi CSDL trước, thành công mới đổi dòng trên DataGrid.
                _repo.Update(new LoaiThuoc { MaLoai = SelectedItem.MaLoai, TenLoai = ten });
                SelectedItem.TenLoai = ten;
            }
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            ErrorMessage = "Tên loại này đã tồn tại.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Lưu không thành công, kiểm tra kết nối CSDL rồi thử lại.";
        }
    }

    private void Delete()
    {
        if (SelectedItem == null) return;
        try
        {
            _repo.Delete(SelectedItem.MaLoai);
            Items.Remove(SelectedItem);
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            ErrorMessage = "Không xóa được vì loại này đang có thuốc.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Xóa không thành công, kiểm tra kết nối CSDL rồi thử lại.";
        }
    }

    // Nút "Thêm mới": bỏ chọn dòng và xóa trắng form, lần Lưu tiếp theo sẽ là INSERT.
    private void ClearForm()
    {
        SelectedItem = null;
        TenLoai = "";
        ErrorMessage = "";
    }
}
