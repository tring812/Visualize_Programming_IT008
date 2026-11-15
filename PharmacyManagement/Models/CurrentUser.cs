namespace PharmacyManagement.Models;

// Thông tin người đã đăng nhập, dùng chung trong một lần chạy ứng dụng.
public static class CurrentUser
{
    public static int MaNV { get; set; }

    public static string HoTen { get; set; } = "";

    public static string VaiTro { get; set; } = "";

    public static bool LaQuanLy => VaiTro == "QuanLy";
}
