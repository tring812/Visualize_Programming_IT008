# Dữ liệu mẫu và tài khoản admin (T06)

## 1. Mục tiêu

Tạo dữ liệu ban đầu cho 9 bảng để cả nhóm kiểm tra chức năng quản lý tiệm thuốc. Có sẵn tài khoản admin, giao dịch nhập/bán và dữ liệu cảnh báo tồn kho, hạn dùng.

## 2. File đã làm

- `database/02_seed.sql`: script SQL chèn 60 dòng dữ liệu mẫu, gồm tài khoản, danh mục và giao dịch.
- `docs/du-lieu-mau-tai-khoan-admin.md`: tài liệu giải thích để chuẩn bị vấn đáp.

## 3. Khái niệm

- **Seed**: dữ liệu mẫu được chèn bằng `INSERT INTO ... VALUES`. Có 5 dòng ở mỗi bảng `LoaiThuoc`, `NhaCungCap`, `KhachHang`, `NhanVien`, `PhieuNhap`, `HoaDon`; 10 dòng ở mỗi bảng `Thuoc`, `CT_PhieuNhap`, `CT_HoaDon`.
- **Unicode**: `N'Kháng sinh'` giữ dấu tiếng Việt khi ghi vào cột `NVARCHAR`.
- **Khóa và mã tự tăng**: `IDENTITY` tự cấp mã. Bảng cha phải có dữ liệu trước bảng con, ví dụ chèn loại thuốc trước thuốc vì `Thuoc.MaLoai` là khóa ngoại. Hai bảng chi tiết dùng khóa chính ghép giữa mã phiếu/hóa đơn và mã thuốc.
- **Transaction**: các lệnh chèn được lưu cùng nhau bằng `COMMIT`. Nếu có lỗi, `CATCH` kiểm tra `@@TRANCOUNT > 0` để `ROLLBACK`, rồi `THROW` báo lại lỗi. `XACT_ABORT ON` hỗ trợ hủy transaction khi gặp lỗi thực thi; `NOCOUNT ON` ẩn thông báo số dòng bị tác động.
- **Ngày tương đối**: `@NgayHienTai` lấy ngày chạy script; `DATEADD` cộng/trừ ngày để tạo ngày nhập, ngày bán và hạn dùng. Panadol, Hapacol, Smecta còn hạn lần lượt 7, 14, 28 ngày; Loratadin và Cetirizin có tồn 7 và 9 để kiểm tra sắp hết hàng.
- **Tổng tiền và tồn kho**: tổng tiền phiếu bằng tổng `SoLuong × DonGia` (hoặc `GiaNhap`). Seed ghi sẵn `Thuoc.SoLuongTon` bằng tổng nhập trừ tổng bán; không cộng/trừ lại khi chèn chi tiết.

Tài khoản mẫu:

| Tên đăng nhập | Mật khẩu | Vai trò | Trạng thái |
| --- | --- | --- | --- |
| `admin` | `admin123` | `QuanLy` | Hoạt động |
| `nv01`, `nv02`, `nv03` | `nhanvien123` | `NhanVien` | Hoạt động |
| `nv04` | `nhanvien123` | `NhanVien` | Đã khóa |

Mật khẩu lưu nguyên văn trong cột `MatKhau`, tối thiểu 6 ký tự. `TrangThai = 1` là hoạt động, `0` là đã khóa.

## 4. Luồng hoạt động

1. Chạy `01_schema.sql` để tạo lại `PharmacyDB` (xóa dữ liệu cũ), rồi chạy toàn bộ `02_seed.sql`.
2. Seed chọn CSDL bằng `USE PharmacyDB`, mở transaction và chèn theo thứ tự: loại thuốc, nhà cung cấp, khách hàng, nhân viên → thuốc → phiếu nhập và chi tiết → hóa đơn và chi tiết.
3. Thành công thì lưu cả bộ dữ liệu; gặp lỗi thì hoàn tác. Ví dụ Panadol nhập 200 viên, bán 20 viên, nên tồn được ghi là 180 viên.
4. Khi mở màn hình, Repository đọc dữ liệu cho ViewModel; `ObservableCollection` cung cấp danh sách để View hiển thị qua Binding. Các nút gọi Command của ViewModel để xử lý thao tác tương ứng.

## 5. Câu hỏi vấn đáp có thể gặp

**Vì sao phải chạy schema trước seed?**

Schema tạo bảng và ràng buộc; seed dựa vào mã tự tăng bắt đầu từ 1 trên CSDL vừa tạo. Chạy lại riêng seed có thể trùng dữ liệu hoặc sai tham chiếu mã.

**Vì sao chèn bảng cha trước bảng con?**

Để mã được tham chiếu đã tồn tại, tránh vi phạm khóa ngoại.

**Vì sao dùng transaction?**

Để lưu đầy đủ bộ dữ liệu hoặc hoàn tác khi có lỗi, tránh chỉ chèn được một phần.

**Đăng nhập cần kiểm tra gì với dữ liệu mẫu?**

So `TenDangNhap`, `MatKhau` bằng tham số SQL và kiểm tra `TrangThai = 1`; `nv04` dùng thử trường hợp tài khoản bị khóa.

**Làm sao kiểm tra tổng tiền và tồn kho?**

Cộng số lượng nhân đơn giá từng dòng để đối chiếu tổng tiền; lấy tổng nhập trừ tổng bán để đối chiếu tồn kho.

**Hóa đơn khách lẻ được lưu thế nào?**

Hóa đơn số 5 có `MaKH = NULL`, nghĩa là không gắn với khách hàng trong danh mục.
