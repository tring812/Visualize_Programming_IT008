using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Data;
using PharmacyManagement.Models;

namespace PharmacyManagement.ViewModels;

public class NhaCungCapViewModel : BaseViewModel
{
    private readonly NhaCungCapRepository _repo = new();
    private static readonly Regex MauSoDienThoai = new(@"\A[0-9]{10}\z");

    public ObservableCollection<NhaCungCap> Items { get; } = new();

    private NhaCungCap? _selectedItem;
    public NhaCungCap? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            TenNCC = value?.TenNCC ?? "";
            DiaChi = value?.DiaChi ?? "";
            SoDienThoai = value?.SoDienThoai ?? "";
            ErrorMessage = value == null ? "" : ValidateForm();
        }
    }

    private string _tenNCC = "";
    public string TenNCC
    {
        get => _tenNCC;
        set
        {
            if (SetProperty(ref _tenNCC, value))
                ErrorMessage = ValidateForm();
        }
    }

    private string _diaChi = "";
    public string DiaChi
    {
        get => _diaChi;
        set
        {
            if (SetProperty(ref _diaChi, value))
                ErrorMessage = ValidateForm();
        }
    }

    private string _soDienThoai = "";
    public string SoDienThoai
    {
        get => _soDienThoai;
        set
        {
            if (SetProperty(ref _soDienThoai, value))
                ErrorMessage = ValidateForm();
        }
    }

    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public RelayCommand AddCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public NhaCungCapViewModel()
    {
        AddCommand = new RelayCommand(_ => ClearForm());
        SaveCommand = new RelayCommand(_ => Save(), _ => ValidateForm().Length == 0);
        DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);

        try
        {
            foreach (var item in _repo.GetAll())
                Items.Add(item);
        }
        catch (SqlException)
        {
            ErrorMessage = "Không tải được nhà cung cấp. Kiểm tra kết nối SQL Server và cơ sở dữ liệu PharmacyDB.";
        }
    }

    private string ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(TenNCC))
            return "Vui lòng nhập tên nhà cung cấp.";
        if (TenNCC.Trim().Length > 200)
            return "Tên nhà cung cấp tối đa 200 ký tự.";
        if (!MauSoDienThoai.IsMatch(SoDienThoai))
            return "Số điện thoại phải gồm đúng 10 chữ số từ 0 đến 9.";
        if (DiaChi.Trim().Length > 255)
            return "Địa chỉ tối đa 255 ký tự.";
        return "";
    }

    private void Save()
    {
        ErrorMessage = ValidateForm();
        if (ErrorMessage.Length != 0)
            return;

        var selected = SelectedItem;
        var item = new NhaCungCap
        {
            MaNCC = selected?.MaNCC ?? 0,
            TenNCC = TenNCC.Trim(),
            DiaChi = DiaChi.Trim(),
            SoDienThoai = SoDienThoai
        };

        try
        {
            if (selected == null)
            {
                item.MaNCC = _repo.Insert(item);
                Items.Add(item);
            }
            else
            {
                if (!_repo.Update(item))
                {
                    Items.Remove(selected);
                    ClearForm();
                    ErrorMessage = "Nhà cung cấp này không còn trong cơ sở dữ liệu.";
                    return;
                }

                selected.TenNCC = item.TenNCC;
                selected.DiaChi = item.DiaChi;
                selected.SoDienThoai = item.SoDienThoai;
            }
            ClearForm();
        }
        catch (SqlException)
        {
            ErrorMessage = "Lưu nhà cung cấp không thành công. Kiểm tra cơ sở dữ liệu rồi thử lại.";
        }
    }

    private void Delete()
    {
        var selected = SelectedItem;
        if (selected == null)
            return;

        try
        {
            _repo.Delete(selected.MaNCC);
            Items.Remove(selected);
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            ErrorMessage = "Không thể xóa nhà cung cấp này vì đã có phiếu nhập.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Xóa nhà cung cấp không thành công. Kiểm tra cơ sở dữ liệu rồi thử lại.";
        }
    }

    private void ClearForm()
    {
        SelectedItem = null;
        TenNCC = "";
        DiaChi = "";
        SoDienThoai = "";
        ErrorMessage = "";
    }
}
