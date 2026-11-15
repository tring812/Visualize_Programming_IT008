USE PharmacyDB;
GO

-- T06: Dữ liệu mẫu cho 9 bảng. Chạy sau database/01_schema.sql trên CSDL vừa tạo.
-- Tài khoản quản lý: admin / admin123.
-- Tài khoản nhân viên: nv01, nv02, nv03, nv04 / nhanvien123; nv04 đã bị khóa.
-- MatKhauHash dùng SHA-256 trên UTF-8, biểu diễn bằng 64 ký tự hex viết hoa.
-- Lệnh tính hash admin trong PowerShell:
-- [BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes('admin123'))).Replace('-','')
-- Giá nhập, giá bán (VNĐ theo Viên/Gói), hạn dùng, thông tin liên hệ và giao dịch là dữ liệu giả lập.
-- Các mã IDENTITY bên dưới tương ứng thứ tự INSERT khi chạy ngay sau 01_schema.sql.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @NgayHienTai DATE = CAST(GETDATE() AS DATE);

BEGIN TRY
    BEGIN TRANSACTION;

    -- Bảng cha: mỗi bảng 5 dòng.
    INSERT INTO dbo.LoaiThuoc (TenLoai)
    VALUES
        (N'Giảm đau, hạ sốt'),
        (N'Kháng sinh'),
        (N'Chống dị ứng'),
        (N'Tiêu hóa'),
        (N'Hô hấp');

    INSERT INTO dbo.NhaCungCap (TenNCC, DiaChi, SoDienThoai)
    VALUES
        (N'Nhà phân phối dược An Tâm (mẫu)', N'12 Nguyễn Trãi, TP. Hồ Chí Minh', '02800000001'),
        (N'Nhà phân phối dược Bình An (mẫu)', N'25 Lê Lợi, TP. Hồ Chí Minh', '02800000002'),
        (N'Nhà phân phối dược Minh Châu (mẫu)', N'38 Hai Bà Trưng, TP. Hồ Chí Minh', '02800000003'),
        (N'Nhà phân phối dược Phúc Khang (mẫu)', N'46 Võ Văn Tần, TP. Hồ Chí Minh', '02800000004'),
        (N'Nhà phân phối dược Thành Đạt (mẫu)', N'59 Cách Mạng Tháng Tám, TP. Hồ Chí Minh', '02800000005');

    INSERT INTO dbo.KhachHang (HoTen, SoDienThoai, DiaChi)
    VALUES
        (N'Nguyễn Minh Anh', '0900000001', N'15 Nguyễn Đình Chiểu, TP. Hồ Chí Minh'),
        (N'Trần Quốc Bảo', '0900000002', N'27 Phan Đăng Lưu, TP. Hồ Chí Minh'),
        (N'Lê Ngọc Châu', '0900000003', N'39 Hoàng Văn Thụ, TP. Hồ Chí Minh'),
        (N'Phạm Thanh Dũng', '0900000004', N'48 Điện Biên Phủ, TP. Hồ Chí Minh'),
        (N'Võ Thị Hồng', '0900000005', N'61 Lý Thường Kiệt, TP. Hồ Chí Minh');

    INSERT INTO dbo.NhanVien (HoTen, SoDienThoai, TenDangNhap, MatKhauHash, VaiTro, TrangThai)
    VALUES
        (N'Nguyễn Hoàng Nam', '0910000001', N'admin', '240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9', N'QuanLy', 1),
        (N'Trần Thị Mai', '0910000002', N'nv01', '42FAE8A13D42327ABBB8D700B0E2A7452F4198A63DFD3C88D6D052AD6AEE7F20', N'NhanVien', 1),
        (N'Lê Minh Tuấn', '0910000003', N'nv02', '42FAE8A13D42327ABBB8D700B0E2A7452F4198A63DFD3C88D6D052AD6AEE7F20', N'NhanVien', 1),
        (N'Phạm Ngọc Linh', '0910000004', N'nv03', '42FAE8A13D42327ABBB8D700B0E2A7452F4198A63DFD3C88D6D052AD6AEE7F20', N'NhanVien', 1),
        (N'Võ Quốc Huy', '0910000005', N'nv04', '42FAE8A13D42327ABBB8D700B0E2A7452F4198A63DFD3C88D6D052AD6AEE7F20', N'NhanVien', 0);

    -- Tên, hàm lượng và dạng thuốc đối chiếu từ nguồn chính hãng.
    -- Panadol 500 mg: https://www.panadol.com/content/cf-consumer-healthcare/wellness-panadol/vi_vn/home/san-pham/san-pham-nguoi-lon/giam-dau-va-ha-sot/panadol-.html
    -- Hapacol 650: https://dhgpharma.com.vn/en/product/analgesic-antipyretic/hapacol-650
    -- Amoxicillin 500 mg: https://domesco.com/pdf/Amoxicillin%20500%20mg%20%28do-vang%29%20192%20GH%20%5B893110107324%5D%20PP125D-1.6b03e589bbef83a217a3144aa2f5b9d8.pdf
    -- Hapenxin capsules 500 mg: https://dhgpharma.com.vn/en/product/antibiotic/hapenxin-capsules-xanh-tim
    -- Loratadin 10 mg: https://domesco.com/san-pham/thuoc-hoa-duoc/thuoc-khac/chong-di-ung-va-dung-trong-cac-truong-hop-qua-man/dopolys---s.html
    -- Cetirizin 10 mg: https://domesco.com/san-pham/thuoc-hoa-duoc/thuoc-khac/chong-di-ung-va-dung-trong-cac-truong-hop-qua-man/cetirizin.html
    -- Omeprazol 20 mg: https://domesco.com/san-pham/thuoc-hoa-duoc/thuoc-duong-tieu-hoa/khang-axit-va-thuoc-chong-loet-khac/omeprazol-20mg.html
    -- Smecta 3 g: https://smecta.com.vn/product/smecta/
    -- Bromhexin 8 mg: https://domesco.com/san-pham/thuoc-hoa-duoc/ho-hap/thuoc-chua-ho/bromhexin-8mg-vien-nang.html
    -- Mitux 200 mg: https://dhgpharma.com.vn/vi/san-pham/ho-hap/mitux
    -- MaThuoc 1, 2, 8 sắp hết hạn sau 7, 14, 28 ngày tính từ lúc chạy seed.
    -- MaThuoc 5, 6 sắp hết hàng với số lượng tồn 7 và 9.
    -- Tồn kho cuối kỳ = tổng số lượng nhập - tổng số lượng bán của từng thuốc.
    INSERT INTO dbo.Thuoc (TenThuoc, DonViTinh, GiaBan, SoLuongTon, HanSuDung, MaLoai)
    VALUES
        (N'Panadol 500 mg', N'Viên', 1500, 180, DATEADD(DAY, 7, @NgayHienTai), 1),
        (N'Hapacol 650', N'Viên', 2000, 135, DATEADD(DAY, 14, @NgayHienTai), 1),
        (N'Amoxicillin 500 mg DOMESCO', N'Viên', 1500, 90, DATEADD(DAY, 365, @NgayHienTai), 2),
        (N'Hapenxin capsules 500 mg', N'Viên', 2500, 72, DATEADD(DAY, 450, @NgayHienTai), 2),
        (N'Loratadin 10 mg DOMESCO', N'Viên', 1000, 7, DATEADD(DAY, 300, @NgayHienTai), 3),
        (N'Cetirizin 10 mg DOMESCO', N'Viên', 1200, 9, DATEADD(DAY, 360, @NgayHienTai), 3),
        (N'Omeprazol 20 mg DOMESCO', N'Viên', 1500, 90, DATEADD(DAY, 540, @NgayHienTai), 4),
        (N'Smecta 3 g', N'Gói', 5000, 55, DATEADD(DAY, 28, @NgayHienTai), 4),
        (N'Bromhexin 8 mg DOMESCO', N'Viên', 1000, 108, DATEADD(DAY, 365, @NgayHienTai), 5),
        (N'Mitux 200 mg', N'Gói', 2500, 90, DATEADD(DAY, 420, @NgayHienTai), 5);

    -- 5 phiếu nhập, mỗi phiếu có 2 dòng chi tiết. Nhập trước ngày bán.
    INSERT INTO dbo.PhieuNhap (NgayNhap, TongTien, MaNCC, MaNV)
    VALUES
        (DATEADD(DAY, -90, @NgayHienTai), 410000, 1, 1),
        (DATEADD(DAY, -85, @NgayHienTai), 244000, 2, 2),
        (DATEADD(DAY, -80, @NgayHienTai), 19500, 3, 3),
        (DATEADD(DAY, -75, @NgayHienTai), 310000, 4, 4),
        (DATEADD(DAY, -70, @NgayHienTai), 242000, 5, 1);

    INSERT INTO dbo.CT_PhieuNhap (MaPN, MaThuoc, SoLuong, GiaNhap)
    VALUES
        (1, 1, 200, 1000),
        (1, 2, 150, 1400),
        (2, 3, 100, 1000),
        (2, 4, 80, 1800),
        (3, 5, 15, 600),
        (3, 6, 15, 700),
        (4, 7, 100, 1000),
        (4, 8, 60, 3500),
        (5, 9, 120, 600),
        (5, 10, 100, 1700);

    -- 5 hóa đơn, mỗi hóa đơn có 2 dòng chi tiết; hóa đơn 5 là khách lẻ.
    -- TongTien = tổng SoLuong * DonGia trong chi tiết; giá bán theo Viên/Gói.
    INSERT INTO dbo.HoaDon (NgayBan, TongTien, MaNV, MaKH)
    VALUES
        (DATEADD(DAY, -5, @NgayHienTai), 45000, 2, 1),
        (DATEADD(DAY, -4, @NgayHienTai), 50000, 3, 2),
        (DATEADD(DAY, -3, @NgayHienTai), 23000, 4, 3),
        (DATEADD(DAY, -2, @NgayHienTai), 32200, 2, 4),
        (DATEADD(DAY, -1, @NgayHienTai), 37000, 1, NULL);

    INSERT INTO dbo.CT_HoaDon (MaHD, MaThuoc, SoLuong, DonGia)
    VALUES
        (1, 1, 20, 1500),
        (1, 3, 10, 1500),
        (2, 2, 15, 2000),
        (2, 4, 8, 2500),
        (3, 5, 8, 1000),
        (3, 7, 10, 1500),
        (4, 6, 6, 1200),
        (4, 8, 5, 5000),
        (5, 9, 12, 1000),
        (5, 10, 10, 2500);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
