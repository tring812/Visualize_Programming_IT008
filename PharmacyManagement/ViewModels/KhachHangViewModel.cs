using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Data;
using PharmacyManagement.Models;

namespace PharmacyManagement.ViewModels;

public class KhachHangViewModel : BaseViewModel
{
    private readonly KhachHangRepository _repo = new();
    private string _tienToDangLoc = "";

    public ObservableCollection<KhachHang> Items { get; } = new();

    private KhachHang? _selectedItem;
    public KhachHang? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            HoTen = value?.HoTen ?? "";
            DiaChi = value?.DiaChi ?? "";
            SoDienThoai = value?.SoDienThoai ?? "";
            ErrorMessage = value == null ? "" : ValidateForm();
        }
    }

    private string _hoTen = "";
    public string HoTen
    {
        get => _hoTen;
        set
        {
            if (SetProperty(ref _hoTen, value))
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

    private string _sdtTimKiem = "";
    public string SdtTimKiem { get => _sdtTimKiem; set => SetProperty(ref _sdtTimKiem, value); }

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public KhachHangViewModel()
    {
        AddCommand = new RelayCommand(_ => ClearForm());
        SaveCommand = new RelayCommand(_ => Save(), _ => ValidateForm().Length == 0);
        DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);
        SearchCommand = new RelayCommand(_ => LoadCustomers(SdtTimKiem.Trim()));
        LoadCustomers("");
    }

    private void LoadCustomers(string tienTo)
    {
        if (tienTo.Length > 20 || !tienTo.All(char.IsAsciiDigit))
        {
            ErrorMessage = "Nhập tối đa 20 chữ số để tìm theo số điện thoại.";
            return;
        }
        try
        {
            var items = tienTo.Length == 0 ? _repo.GetAll() : _repo.SearchBySdt(tienTo);
            _tienToDangLoc = tienTo;
            ClearForm();
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        catch (SqlException)
        {
            ErrorMessage = "Không tải được khách hàng. Kiểm tra kết nối SQL Server và cơ sở dữ liệu PharmacyDB.";
        }
    }

    private string ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(HoTen))
            return "Vui lòng nhập họ tên.";
        if (HoTen.Trim().Length > 100)
            return "Họ tên tối đa 100 ký tự.";
        if (SoDienThoai.Length is < 1 or > 20 || !SoDienThoai.All(char.IsAsciiDigit))
            return "Số điện thoại phải gồm từ 1 đến 20 chữ số.";
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
        var item = new KhachHang
        {
            MaKH = selected?.MaKH ?? 0,
            HoTen = HoTen.Trim(),
            DiaChi = DiaChi.Trim(),
            SoDienThoai = SoDienThoai
        };

        try
        {
            if (selected == null)
            {
                item.MaKH = _repo.Insert(item);
                if (item.SoDienThoai.StartsWith(_tienToDangLoc, StringComparison.Ordinal))
                    Items.Add(item);
            }
            else
            {
                if (!_repo.Update(item))
                {
                    Items.Remove(selected);
                    ClearForm();
                    ErrorMessage = "Khách hàng này không còn trong cơ sở dữ liệu.";
                    return;
                }

                selected.HoTen = item.HoTen;
                selected.DiaChi = item.DiaChi;
                selected.SoDienThoai = item.SoDienThoai;
                if (!item.SoDienThoai.StartsWith(_tienToDangLoc, StringComparison.Ordinal))
                    Items.Remove(selected);
            }
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            ErrorMessage = "Số điện thoại đã có.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Lưu khách hàng không thành công. Kiểm tra cơ sở dữ liệu rồi thử lại.";
        }
    }

    private void Delete()
    {
        var selected = SelectedItem;
        if (selected == null)
            return;

        try
        {
            _repo.Delete(selected.MaKH);
            Items.Remove(selected);
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            ErrorMessage = "Không thể xóa khách hàng này vì đã có hóa đơn.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Xóa khách hàng không thành công. Kiểm tra cơ sở dữ liệu rồi thử lại.";
        }
    }

    private void ClearForm()
    {
        SelectedItem = null;
        HoTen = "";
        DiaChi = "";
        SoDienThoai = "";
        ErrorMessage = "";
    }
}
