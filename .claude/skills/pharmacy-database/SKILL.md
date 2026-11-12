---
name: pharmacy-database
description: Quy tắc CSDL SQL Server của đồ án Quản lý tiệm thuốc — 9 bảng, script database/01_schema.sql và 02_seed.sql, ràng buộc, transaction cho hóa đơn và phiếu nhập, băm mật khẩu, cảnh báo hạn dùng. Dùng skill này mỗi khi người dùng tạo hoặc sửa bảng, cột, khóa, dữ liệu mẫu, viết câu SELECT/INSERT/UPDATE/JOIN/GROUP BY, viết Repository có SQL, lưu hóa đơn hay phiếu nhập, cộng trừ tồn kho, hoặc gặp lỗi SqlException, lỗi kết nối, lỗi khóa ngoại — kể cả khi chỉ nói "database", "SQL", "bảng", "ERD", "seed", "tồn kho".
---

# CSDL đồ án Quản lý tiệm thuốc

CSDL tên `PharmacyDB` trên SQL Server Express, instance `.\SQLEXPRESS`. Mỗi máy chạy CSDL riêng, tạo lại hoàn toàn từ hai script trong `database/`. Script là nguồn sự thật duy nhất: ai sửa bảng trực tiếp trong SSMS mà không cập nhật script thì máy của bốn bạn còn lại sẽ lỗi.

## Chín bảng

| Bảng | Cột chính | Quan hệ |
| --- | --- | --- |
| `NhanVien` | `MaNV` PK, `HoTen`, `SoDienThoai`, `TenDangNhap` UNIQUE, `MatKhauHash`, `VaiTro`, `TrangThai` BIT | |
| `KhachHang` | `MaKH` PK, `HoTen`, `SoDienThoai` UNIQUE, `DiaChi` | |
| `LoaiThuoc` | `MaLoai` PK IDENTITY, `TenLoai` | |
| `NhaCungCap` | `MaNCC` PK, `TenNCC`, `DiaChi`, `SoDienThoai` | |
| `Thuoc` | `MaThuoc` PK, `TenThuoc`, `DonViTinh`, `GiaBan`, `SoLuongTon`, `HanSuDung` | `MaLoai` → `LoaiThuoc` |
| `PhieuNhap` | `MaPN` PK, `NgayNhap`, `TongTien` | `MaNCC` → `NhaCungCap`, `MaNV` → `NhanVien` |
| `CT_PhieuNhap` | `SoLuong`, `GiaNhap` | PK (`MaPN`, `MaThuoc`) |
| `HoaDon` | `MaHD` PK, `NgayBan`, `TongTien` | `MaNV` → `NhanVien`, `MaKH` → `KhachHang` (NULL = khách lẻ) |
| `CT_HoaDon` | `SoLuong`, `DonGia` | PK (`MaHD`, `MaThuoc`) |

Phạm vi đã chốt: không thêm bảng lô hàng, tích điểm, chi nhánh. Nếu người dùng muốn thêm bảng hay cột, nhắc họ rằng việc này ảnh hưởng cả nhóm và cần báo trưởng nhóm cùng người giữ script trước.

## Quy ước

- Tên bảng và cột: tiếng Việt không dấu, PascalCase (`SoLuongTon`). Khóa chính `Ma…`.
- Tiền: `DECIMAL(18,0)` (đồng, không có số lẻ). Số lượng: `INT`. Ngày: `DATE` cho hạn dùng, `DATETIME` cho ngày bán/nhập.
- Chữ tiếng Việt: `NVARCHAR`, và khi chèn dữ liệu viết `N'Thuốc ho'`. Thiếu `N` sẽ ra dấu `?`.
- Ràng buộc để SQL Server tự chặn dữ liệu sai: `CHECK (SoLuongTon >= 0)`, `CHECK (GiaBan > 0)`, `CHECK (SoLuong > 0)`, `CHECK (VaiTro IN (N'QuanLy', N'NhanVien'))`.
- Khóa ngoại để mặc định (không `ON DELETE CASCADE`): xóa loại thuốc đang có thuốc hay thuốc đã bán phải bị chặn, rồi C# báo câu dễ hiểu (lỗi số 547).

## Viết và sửa script

`database/01_schema.sql` phải chạy lại được từ đầu trên máy sạch:

```sql
USE master;
IF DB_ID(N'PharmacyDB') IS NOT NULL
BEGIN
    ALTER DATABASE PharmacyDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PharmacyDB;
END
GO
CREATE DATABASE PharmacyDB;
GO
USE PharmacyDB;
GO
-- Tạo bảng theo thứ tự: bảng không phụ thuộc trước, bảng có khóa ngoại sau
```

`database/02_seed.sql` chèn dữ liệu mẫu và một tài khoản Quản lý `admin` để đăng nhập lần đầu. `MatKhauHash` của admin phải là chuỗi SHA-256 (hex) đúng như C# tạo ra, nên tính bằng chính hàm băm trong code rồi dán vào, và ghi mật khẩu mẫu trong comment cho nhóm biết.

Khi sửa script: sửa cả hai file cho khớp, chạy lại cả hai trong SSMS, rồi ghi rõ trong commit và báo nhóm "chạy lại script" vì CSDL cũ của mọi người sẽ lệch.

## Quy tắc nghiệp vụ

**Tồn kho** nằm ở `Thuoc.SoLuongTon`, không có bảng tồn kho riêng. Nhập hàng cộng, bán hàng trừ.

**Hóa đơn và phiếu nhập lưu trong một `SqlTransaction`**: ghi phần đầu, ghi các dòng chi tiết, cập nhật tồn. Lỗi ở bất kỳ bước nào thì `Rollback` toàn bộ, để không bao giờ có hóa đơn mà tồn kho chưa trừ. Mẫu đầy đủ: [references/transaction.md](references/transaction.md).

**Chặn bán quá tồn ngay trong câu UPDATE**, không chỉ kiểm tra trước ở C#, vì giữa lúc kiểm tra và lúc ghi số liệu có thể đã đổi:

```sql
UPDATE Thuoc SET SoLuongTon = SoLuongTon - @SoLuong
WHERE MaThuoc = @MaThuoc AND SoLuongTon >= @SoLuong AND HanSuDung >= CAST(GETDATE() AS DATE);
```

Nếu `ExecuteNonQuery()` trả về 0 thì thuốc không đủ hoặc đã hết hạn → `Rollback` và báo người dùng.

**`CT_HoaDon.DonGia`** lưu giá tại thời điểm bán, vì `Thuoc.GiaBan` có thể đổi về sau.

**Mật khẩu**: không lưu mật khẩu gốc. C# băm bằng SHA-256:

```csharp
using System.Security.Cryptography;
using System.Text;

public static string HashPassword(string password)
{
    byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    return Convert.ToHexString(bytes);   // 64 ký tự hex, cột NVARCHAR(64) hoặc CHAR(64)
}
```

Đăng nhập: băm mật khẩu vừa nhập rồi so với `MatKhauHash`, và kiểm tra `TrangThai = 1`.

**Tài khoản**: tự đăng ký luôn là `N'NhanVien'`; chỉ Quản lý đổi được vai trò. Nhân viên đã lập hóa đơn hay phiếu nhập thì không xóa, chỉ khóa bằng `TrangThai = 0`.

**Hạn dùng**: mỗi thuốc một hạn dùng (không quản lý theo lô).
- Cảnh báo: thuốc còn tồn có `HanSuDung <= DATEADD(DAY, 30, CAST(GETDATE() AS DATE))`.
- Đã hết hạn: `HanSuDung < CAST(GETDATE() AS DATE)` — chặn khi bán.

## Câu truy vấn hay dùng

```sql
-- Danh sách thuốc kèm tên loại (JOIN)
SELECT t.MaThuoc, t.TenThuoc, l.TenLoai, t.DonViTinh, t.GiaBan, t.SoLuongTon, t.HanSuDung
FROM Thuoc t JOIN LoaiThuoc l ON l.MaLoai = t.MaLoai
WHERE (@TuKhoa IS NULL OR t.TenThuoc LIKE N'%' + @TuKhoa + N'%')
  AND (@MaLoai IS NULL OR t.MaLoai = @MaLoai)
ORDER BY t.TenThuoc;

-- Doanh thu theo ngày (GROUP BY)
SELECT CAST(NgayBan AS DATE) AS Ngay, COUNT(*) AS SoHoaDon, SUM(TongTien) AS DoanhThu
FROM HoaDon GROUP BY CAST(NgayBan AS DATE) ORDER BY Ngay DESC;
```

`LIKE` với tham số vẫn an toàn vì chuỗi tìm kiếm đi qua `@TuKhoa`, không nối vào câu SQL.

## Lỗi thường gặp

| Triệu chứng | Nguyên nhân hay gặp |
| --- | --- |
| `A network-related or instance-specific error` | SQL Server chưa chạy hoặc tên instance không phải `SQLEXPRESS` |
| `Cannot open database "PharmacyDB"` | Chưa chạy `01_schema.sql` trên máy này |
| Chữ tiếng Việt thành `?` | Cột `VARCHAR` thay vì `NVARCHAR`, hoặc thiếu `N'...'` |
| `SqlException.Number == 547` | Vi phạm khóa ngoại hoặc CHECK |
| `2627` / `2601` | Trùng khóa chính hoặc UNIQUE |
| Tồn kho âm hoặc lệch | Không dùng transaction, hoặc không kiểm tra `SoLuongTon >= @SoLuong` trong UPDATE |
