using System;
using PharmacyManagement.ViewModels;

namespace PharmacyManagement.Models;

public class Thuoc : BaseViewModel
{
    public int MaThuoc { get; set; }

    private string _tenThuoc = "";
    public string TenThuoc { get => _tenThuoc; set => SetProperty(ref _tenThuoc, value); }

    private string _donViTinh = "";
    public string DonViTinh { get => _donViTinh; set => SetProperty(ref _donViTinh, value); }

    private decimal _giaBan;
    public decimal GiaBan { get => _giaBan; set => SetProperty(ref _giaBan, value); }

    private int _soLuongTon;
    public int SoLuongTon { get => _soLuongTon; set => SetProperty(ref _soLuongTon, value); }

    private DateTime _hanSuDung;
    public DateTime HanSuDung { get => _hanSuDung; set => SetProperty(ref _hanSuDung, value); }

    private int _maLoai;
    public int MaLoai { get => _maLoai; set => SetProperty(ref _maLoai, value); }

    private string _tenLoai = "";
    public string TenLoai { get => _tenLoai; set => SetProperty(ref _tenLoai, value); }
}
