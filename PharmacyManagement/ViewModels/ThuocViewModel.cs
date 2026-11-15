using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Data;
using PharmacyManagement.Models;

namespace PharmacyManagement.ViewModels;

public class ThuocViewModel : BaseViewModel
{
    private const decimal GiaBanToiDa = 999_999_999_999_999_999m;
    private readonly ThuocRepository _repo = new();
    private readonly LoaiThuocRepository _loaiRepo = new();

    public ObservableCollection<Thuoc> Items { get; } = new();
    public ObservableCollection<LoaiThuoc> DanhSachLoai { get; } = new();

    private Thuoc? _selectedItem;
    public Thuoc? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            TenThuoc = value?.TenThuoc ?? "";
            DonViTinh = value?.DonViTinh ?? "";
            GiaBan = value?.GiaBan.ToString("0", CultureInfo.CurrentCulture) ?? "";
            SoLuongTon = value?.SoLuongTon.ToString(CultureInfo.CurrentCulture) ?? "0";
            HanSuDung = value?.HanSuDung;
            MaLoai = value?.MaLoai;
            ErrorMessage = value == null ? "" : ValidateForm(out _, out _);
        }
    }

    private string _tenThuoc = "";
    public string TenThuoc
    {
        get => _tenThuoc;
        set
        {
            if (SetProperty(ref _tenThuoc, value))
                ErrorMessage = ValidateForm(out _, out _);
        }
    }

    private string _donViTinh = "";
    public string DonViTinh
    {
        get => _donViTinh;
        set
        {
            if (SetProperty(ref _donViTinh, value))
                ErrorMessage = ValidateForm(out _, out _);
        }
    }

    private string _giaBan = "";
    public string GiaBan
    {
        get => _giaBan;
        set
        {
            if (SetProperty(ref _giaBan, value))
                ErrorMessage = ValidateForm(out _, out _);
        }
    }

    private string _soLuongTon = "0";
    public string SoLuongTon
    {
        get => _soLuongTon;
        set
        {
            if (SetProperty(ref _soLuongTon, value))
                ErrorMessage = ValidateForm(out _, out _);
        }
    }

    private DateTime? _hanSuDung;
    public DateTime? HanSuDung
    {
        get => _hanSuDung;
        set
        {
            if (SetProperty(ref _hanSuDung, value))
                ErrorMessage = ValidateForm(out _, out _);
        }
    }

    private int? _maLoai;
    public int? MaLoai
    {
        get => _maLoai;
        set
        {
            if (SetProperty(ref _maLoai, value))
                ErrorMessage = ValidateForm(out _, out _);
        }
    }

    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public RelayCommand AddCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public ThuocViewModel()
    {
        AddCommand = new RelayCommand(_ => ClearForm());
        SaveCommand = new RelayCommand(_ => Save(), CanSave);
        DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);

        try
        {
            foreach (var loai in _loaiRepo.GetAll())
                DanhSachLoai.Add(loai);
            foreach (var thuoc in _repo.GetAll())
                Items.Add(thuoc);
        }
        catch (SqlException)
        {
            ErrorMessage = "Không tải được danh mục thuốc. Kiểm tra SQL Server và cơ sở dữ liệu PharmacyDB.";
        }
    }

    private bool CanSave(object? parameter) => ValidateForm(out _, out _).Length == 0;

    private string ValidateForm(out decimal giaBan, out int soLuongTon)
    {
        giaBan = 0;
        soLuongTon = 0;
        if (string.IsNullOrWhiteSpace(TenThuoc) || TenThuoc.Trim().Length > 200)
            return "Nhập tên thuốc từ 1 đến 200 ký tự.";
        if (string.IsNullOrWhiteSpace(DonViTinh) || DonViTinh.Trim().Length > 50)
            return "Nhập đơn vị tính từ 1 đến 50 ký tự.";
        if (!decimal.TryParse(GiaBan, NumberStyles.Integer, CultureInfo.CurrentCulture, out giaBan)
            || giaBan <= 0 || giaBan > GiaBanToiDa)
            return "Giá bán phải là số đồng nguyên dương, tối đa 18 chữ số.";
        if (!int.TryParse(SoLuongTon, out soLuongTon) || soLuongTon < 0)
            return "Số lượng tồn phải là số nguyên từ 0 đến 2.147.483.647.";
        if (!DanhSachLoai.Any(loai => loai.MaLoai == MaLoai))
            return "Vui lòng chọn loại thuốc.";
        if (HanSuDung == null)
            return "Vui lòng chọn hạn dùng hợp lệ.";
        return "";
    }

    private void Save()
    {
        ErrorMessage = ValidateForm(out var giaBan, out var soLuongTon);
        if (ErrorMessage.Length != 0)
            return;

        var selected = SelectedItem;
        var loai = DanhSachLoai.First(x => x.MaLoai == MaLoai);
        var item = new Thuoc
        {
            MaThuoc = selected?.MaThuoc ?? 0,
            TenThuoc = TenThuoc.Trim(),
            DonViTinh = DonViTinh.Trim(),
            GiaBan = giaBan,
            SoLuongTon = soLuongTon,
            HanSuDung = HanSuDung!.Value.Date,
            MaLoai = loai.MaLoai,
            TenLoai = loai.TenLoai
        };

        try
        {
            if (selected == null)
            {
                item.MaThuoc = _repo.Insert(item);
                Items.Add(item);
            }
            else
            {
                if (!_repo.Update(item, selected.SoLuongTon))
                {
                    ErrorMessage = "Thuốc đã bị xóa hoặc tồn kho đã đổi. Mở lại màn Thuốc để tải dữ liệu mới.";
                    return;
                }

                selected.TenThuoc = item.TenThuoc;
                selected.DonViTinh = item.DonViTinh;
                selected.GiaBan = item.GiaBan;
                selected.SoLuongTon = item.SoLuongTon;
                selected.HanSuDung = item.HanSuDung;
                selected.MaLoai = item.MaLoai;
                selected.TenLoai = item.TenLoai;
            }
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            ErrorMessage = "Không lưu được thuốc. Kiểm tra loại thuốc còn tồn tại rồi thử lại.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Lưu thuốc không thành công. Kiểm tra cơ sở dữ liệu rồi thử lại.";
        }
    }

    private void Delete()
    {
        var selected = SelectedItem;
        if (selected == null)
            return;

        try
        {
            _repo.Delete(selected.MaThuoc);
            Items.Remove(selected);
            ClearForm();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            ErrorMessage = "Không thể xóa thuốc đã có trong phiếu nhập hoặc hóa đơn.";
        }
        catch (SqlException)
        {
            ErrorMessage = "Xóa thuốc không thành công. Kiểm tra cơ sở dữ liệu rồi thử lại.";
        }
    }

    private void ClearForm()
    {
        SelectedItem = null;
        TenThuoc = "";
        DonViTinh = "";
        GiaBan = "";
        SoLuongTon = "0";
        HanSuDung = null;
        MaLoai = null;
        ErrorMessage = "";
    }
}
