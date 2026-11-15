# Dữ liệu mẫu và tài khoản admin (T06)

## 1. Mục tiêu

Viết `database/02_seed.sql` để cả nhóm có cùng bộ dữ liệu ban đầu cho 9 bảng của ứng dụng quản lý tiệm thuốc. Bộ dữ liệu gồm tài khoản quản lý, nhân viên, danh mục thuốc, khách hàng, nhà cung cấp, giao dịch nhập/bán và các trường hợp phục vụ kiểm tra cảnh báo sắp hết hạn, sắp hết hàng.

## 2. File đã làm

| File | Thành phần | Vai trò |
| --- | --- | --- |
| `database/02_seed.sql` | Script SQL khởi tạo dữ liệu | Chèn 60 dòng vào 9 bảng, lưu mật khẩu dạng SHA-256, tạo dữ liệu giao dịch có tổng tiền và tồn kho khớp nhau |
| `docs/du-lieu-mau-tai-khoan-admin.md` | Tài liệu giải thích | Giải thích từng phần của script, luồng chạy và câu hỏi để chuẩn bị vấn đáp |

Script sử dụng các bảng, cột và ràng buộc được định nghĩa trong `database/01_schema.sql`. Cách khởi tạo của bộ seed là chạy `01_schema.sql` để tạo mới `PharmacyDB`, sau đó chạy toàn bộ `02_seed.sql`.

## 3. Khái niệm

### Dữ liệu mẫu và cách chia dữ liệu cho 9 bảng

**Seed** là bộ dữ liệu ban đầu được viết sẵn bằng SQL. Khi từng thành viên chạy cùng script, mọi người có cùng tài khoản, danh mục và tình huống để phát triển, kiểm tra chức năng.

| Bảng | Số dòng | Nội dung trong T06 |
| --- | --- | --- |
| `LoaiThuoc` | 5 | Giảm đau, hạ sốt; Kháng sinh; Chống dị ứng; Tiêu hóa; Hô hấp |
| `NhaCungCap` | 5 | Năm nhà phân phối mẫu, có tên, địa chỉ và số điện thoại |
| `KhachHang` | 5 | Năm khách hàng có số điện thoại riêng biệt |
| `NhanVien` | 5 | Một quản lý và bốn nhân viên, gồm một tài khoản đã khóa |
| `Thuoc` | 10 | Mỗi loại có hai thuốc, có giá bán, tồn kho và hạn sử dụng |
| `PhieuNhap` | 5 | Mỗi phiếu có nhà cung cấp, nhân viên, ngày nhập và tổng tiền |
| `CT_PhieuNhap` | 10 | Mỗi phiếu nhập có hai thuốc, kèm số lượng và giá nhập |
| `HoaDon` | 5 | Mỗi hóa đơn có nhân viên, ngày bán, tổng tiền và thông tin khách hàng nếu có |
| `CT_HoaDon` | 10 | Mỗi hóa đơn có hai thuốc, kèm số lượng và đơn giá bán |

Tổng cộng có `6 × 5 + 3 × 10 = 60` dòng. Từng bảng đều đáp ứng yêu cầu có từ 5 đến 10 dòng.

### `USE PharmacyDB`, `GO` và `dbo`

Hai dòng đầu của script là

```sql
USE PharmacyDB;
GO
```

`USE PharmacyDB` chọn CSDL mà các câu lệnh tiếp theo sẽ làm việc. Vì vậy, các lệnh `INSERT` trong file được thực hiện trên `PharmacyDB`.

`GO` là dấu phân cách batch mà SSMS hoặc `sqlcmd` nhận biết. Công cụ gửi phần lệnh trước `GO` đến SQL Server rồi xử lý batch tiếp theo. Khi chạy file này bằng SSMS, cần thực thi toàn bộ file để có đủ phần chọn CSDL, khai báo biến và các lệnh chèn dữ liệu.

Trong `dbo.Thuoc`, `dbo` là tên schema chứa bảng, còn `Thuoc` là tên bảng. Viết đầy đủ `dbo.TenBang` giúp xác định rõ đối tượng mà câu lệnh sử dụng.

### `INSERT INTO`, danh sách cột và chuỗi tiếng Việt

Ví dụ chèn các loại thuốc trong script

```sql
INSERT INTO dbo.LoaiThuoc (TenLoai)
VALUES
    (N'Giảm đau, hạ sốt'),
    (N'Kháng sinh'),
    (N'Chống dị ứng'),
    (N'Tiêu hóa'),
    (N'Hô hấp');
```

`INSERT INTO` xác định bảng và cột cần ghi. Mỗi cặp ngoặc trong `VALUES` tương ứng một dòng mới. Với bảng có nhiều cột như `Thuoc`, các giá trị trong từng dòng phải khớp đúng thứ tự cột đã liệt kê.

Tiền tố `N` trong `N'Kháng sinh'` tạo chuỗi Unicode. Các cột tên, địa chỉ và đơn vị tính dùng kiểu `NVARCHAR`, nên script dùng `N'...'` để giữ đầy đủ dấu tiếng Việt khi SQL Server tiếp nhận chuỗi.

Số điện thoại nằm trong dấu nháy, ví dụ `'0900000001'`, vì cột này lưu chuỗi `VARCHAR`. Cách lưu này giữ được số `0` ở đầu. Các giá trị số như `1500`, `180`, `1` được viết trực tiếp theo kiểu dữ liệu của cột.

### `IDENTITY` và thứ tự chèn theo khóa ngoại

Các bảng danh mục và bảng đầu phiếu trong `01_schema.sql` có mã tự tăng `IDENTITY(1,1)`. Trên bảng vừa tạo, mã đầu tiên là `1`, sau đó tăng mỗi lần một đơn vị. Vì vậy, phần `INSERT` của seed bỏ cột mã tự tăng để SQL Server tự cấp.

Script dựa vào thứ tự chèn trên CSDL vừa tạo. Năm loại thuốc lần lượt nhận mã `1` đến `5`. Mười thuốc nhận mã `1` đến `10`. Ví dụ, dòng Panadol có `MaLoai = 1` nên thuộc nhóm Giảm đau, hạ sốt, còn dòng Amoxicillin có `MaLoai = 2` nên thuộc nhóm Kháng sinh.

**Khóa ngoại** bảo đảm mã được tham chiếu đã tồn tại ở bảng liên quan. `Thuoc.MaLoai` phải trỏ đến một dòng của `LoaiThuoc`, còn `CT_HoaDon.MaHD` phải trỏ đến một hóa đơn đã có.

Thứ tự thực tế trong T06 là

```text
LoaiThuoc → NhaCungCap → KhachHang → NhanVien → Thuoc
→ PhieuNhap → CT_PhieuNhap → HoaDon → CT_HoaDon
```

Bốn bảng đầu được chèn trước vì chúng cung cấp dữ liệu cho các bảng phía sau. Khi chèn chi tiết nhập, cả phiếu nhập và thuốc đã tồn tại. Khi chèn chi tiết bán, cả hóa đơn và thuốc đã tồn tại.

Hai bảng chi tiết dùng khóa chính ghép. Ví dụ, cặp `(MaHD, MaThuoc)` phải duy nhất trong `CT_HoaDon`. Hóa đơn `1` có thể có thuốc `1` và thuốc `3`, còn một thuốc có thể xuất hiện ở nhiều hóa đơn khác nhau.

**Điều kiện sử dụng bộ seed** là CSDL vừa được tạo bằng `01_schema.sql`. Script dùng `INSERT` và các mã cố định ở khóa ngoại, nên chạy thêm lần nữa trên CSDL đã có dữ liệu có thể gặp trùng `TenDangNhap`, trùng số điện thoại hoặc sai giả định về mã tự tăng. `01_schema.sql` xóa CSDL cũ và tạo lại từ đầu, nên quy trình hai file này phục vụ việc khởi tạo bộ dữ liệu mẫu.

### Tài khoản, vai trò và trạng thái hoạt động

| `MaNV` | Tên đăng nhập | Mật khẩu mẫu | `VaiTro` | `TrangThai` |
| --- | --- | --- | --- | --- |
| 1 | `admin` | `admin123` | `QuanLy` | `1` |
| 2 | `nv01` | `nhanvien123` | `NhanVien` | `1` |
| 3 | `nv02` | `nhanvien123` | `NhanVien` | `1` |
| 4 | `nv03` | `nhanvien123` | `NhanVien` | `1` |
| 5 | `nv04` | `nhanvien123` | `NhanVien` | `0` |

`VaiTro` dùng đúng hai giá trị mà ràng buộc `CHECK` trong schema cho phép. Tài khoản `admin` có vai trò quản lý, các tài khoản còn lại có vai trò nhân viên.

`TrangThai` là cột `BIT`. Giá trị `1` biểu thị tài khoản hoạt động, giá trị `0` biểu thị tài khoản đã khóa. Dòng `nv04` tạo tình huống để phần đăng nhập kiểm tra tài khoản bị khóa. Các phiếu nhập và hóa đơn trong seed đều tham chiếu nhân viên đang hoạt động.

### SHA-256, UTF-8 và `MatKhauHash`

Script lưu chuỗi băm vào cột `MatKhauHash CHAR(64)`. Mật khẩu mẫu được ghi ở comment đầu file để các thành viên biết thông tin đăng nhập.

Quá trình tạo giá trị lưu trong CSDL đi qua ba bước

```text
"admin123" → các byte UTF-8 → SHA-256 → chuỗi hex viết hoa
```

`UTF-8` là cách chuyển chuỗi thành dãy byte đầu vào. SHA-256 xử lý dãy byte này và tạo kết quả dài 256 bit, tương đương 32 byte. Mỗi byte được biểu diễn bằng hai ký tự hex, nên chuỗi cuối cùng dài `32 × 2 = 64` ký tự, khớp cột `CHAR(64)`.

Lệnh PowerShell đã dùng để tính hash cho quản lý là

```powershell
[BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes('admin123'))).Replace('-','')
```

`GetBytes` chuyển mật khẩu thành byte UTF-8. `ComputeHash` tính SHA-256. `BitConverter.ToString` biểu diễn các byte bằng ký tự hex viết hoa có dấu gạch nối. `Replace('-','')` bỏ các dấu gạch nối để thu được chuỗi 64 ký tự.

Hash của `admin123` được dán vào dòng tài khoản `admin` trong seed

```text
240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9
```

Hash của `nhanvien123` được dùng chung cho bốn tài khoản nhân viên mẫu

```text
42FAE8A13D42327ABBB8D700B0E2A7452F4198A63DFD3C88D6D052AD6AEE7F20
```

Cùng mật khẩu, cùng cách chuyển byte và cùng thuật toán sẽ tạo cùng kết quả băm. Khi phần đăng nhập được triển khai, code cần thực hiện đúng quy ước SHA-256 trên UTF-8, biểu diễn hex viết hoa, rồi so sánh với `MatKhauHash` và kiểm tra `TrangThai = 1`.

### Ngày hiện tại, `DATEADD` và dữ liệu cảnh báo

Đầu script khai báo biến dùng chung cho mọi mốc thời gian

```sql
DECLARE @NgayHienTai DATE = CAST(GETDATE() AS DATE);
```

`GETDATE()` lấy ngày giờ của máy chủ SQL Server. `CAST(... AS DATE)` giữ phần ngày để các phép tính trong seed cùng bắt đầu từ một mốc. Biến `@NgayHienTai` được khai báo một lần và dùng trong toàn bộ batch.

`DATEADD(DAY, 7, @NgayHienTai)` cộng bảy ngày. Số âm như `DATEADD(DAY, -90, @NgayHienTai)` lùi về ngày trong quá khứ. Nhờ vậy, khi một thành viên khởi tạo CSDL ở ngày khác, dữ liệu sắp hết hạn vẫn được tạo tương đối theo ngày chạy seed.

| `MaThuoc` | Tên thuốc | Đơn vị | `MaLoai` | Hạn dùng tính từ ngày chạy seed |
| --- | --- | --- | --- | --- |
| 1 | Panadol 500 mg | Viên | 1 | Cộng 7 ngày |
| 2 | Hapacol 650 | Viên | 1 | Cộng 14 ngày |
| 3 | Amoxicillin 500 mg DOMESCO | Viên | 2 | Cộng 365 ngày |
| 4 | Hapenxin capsules 500 mg | Viên | 2 | Cộng 450 ngày |
| 5 | Loratadin 10 mg DOMESCO | Viên | 3 | Cộng 300 ngày |
| 6 | Cetirizin 10 mg DOMESCO | Viên | 3 | Cộng 360 ngày |
| 7 | Omeprazol 20 mg DOMESCO | Viên | 4 | Cộng 540 ngày |
| 8 | Smecta 3 g | Gói | 4 | Cộng 28 ngày |
| 9 | Bromhexin 8 mg DOMESCO | Viên | 5 | Cộng 365 ngày |
| 10 | Mitux 200 mg | Gói | 5 | Cộng 420 ngày |

Tên, hàm lượng và dạng thuốc được chọn theo các nguồn chính hãng đã ghi ở comment trong `02_seed.sql`. Giá nhập, giá bán, ngày hết hạn, thông tin liên hệ và giao dịch được đặt làm dữ liệu giả lập cho đồ án.

Ngay sau khi seed, Panadol, Hapacol và Smecta đều còn tồn và có hạn sử dụng trong 30 ngày tới. Hai thuốc Loratadin và Cetirizin có tồn lần lượt `7` và `9`, đáp ứng điều kiện sắp hết hàng `SoLuongTon < 10`.

Các ngày hết hạn được tính rồi lưu khi chạy `INSERT`. Qua những ngày tiếp theo, hạn sử dụng vẫn giữ nguyên và số ngày còn lại sẽ giảm dần. Việc xác định màu cảnh báo trên giao diện thuộc luồng xử lý của màn tồn kho khi sử dụng các giá trị này.

### Phiếu nhập, hóa đơn và bảng chi tiết

`PhieuNhap` lưu thông tin chung cho một lần nhập, gồm ngày nhập, nhà cung cấp, người lập và tổng tiền. `CT_PhieuNhap` lưu từng thuốc trong lần nhập đó, gồm số lượng và giá nhập theo đơn vị.

`HoaDon` lưu thông tin chung cho một lần bán. `CT_HoaDon` lưu từng thuốc, số lượng và đơn giá tại thời điểm bán. Ví dụ, hóa đơn `1` có hai dòng, gồm Panadol và Amoxicillin.

Trong seed, các ngày nhập lần lượt cách ngày chạy `90`, `85`, `80`, `75`, `70` ngày về trước. Các ngày bán cách ngày chạy `5`, `4`, `3`, `2`, `1` ngày về trước. Như vậy, toàn bộ hàng trong các giao dịch mẫu đã được nhập trước khi bán và các thuốc đều còn hạn ở ngày bán.

Hóa đơn `5` có `MaKH = NULL`, biểu thị khách lẻ. Cột `HoaDon.MaKH` cho phép `NULL`, nên hóa đơn này vẫn hợp lệ. Với khách có thông tin, các hóa đơn trước đó lưu mã tham chiếu đến `KhachHang`.

### Giá theo đơn vị và cách tính tổng tiền

`GiaBan`, `GiaNhap`, `DonGia` và `TongTien` dùng kiểu `DECIMAL(18,0)`, lưu số tiền theo đồng Việt Nam. Đơn vị tính của thuốc là Viên hoặc Gói, nên số lượng và giá trong chi tiết đều tính theo đúng đơn vị đó.

Ví dụ, Panadol có `DonViTinh = N'Viên'` và `GiaBan = 1500`. Khi bán `20` viên với `DonGia = 1500`, thành tiền của dòng này là `20 × 1.500 = 30.000` đồng.

Công thức tổng tiền phiếu nhập là

```text
TongTien của phiếu nhập = tổng (SoLuong × GiaNhap) của các dòng chi tiết cùng MaPN
```

| `MaPN` | Phép tính từ hai dòng chi tiết | `TongTien` |
| --- | --- | --- |
| 1 | `200 × 1.000 + 150 × 1.400` | 410.000 đồng |
| 2 | `100 × 1.000 + 80 × 1.800` | 244.000 đồng |
| 3 | `15 × 600 + 15 × 700` | 19.500 đồng |
| 4 | `100 × 1.000 + 60 × 3.500` | 310.000 đồng |
| 5 | `120 × 600 + 100 × 1.700` | 242.000 đồng |

Công thức tổng tiền hóa đơn là

```text
TongTien của hóa đơn = tổng (SoLuong × DonGia) của các dòng chi tiết cùng MaHD
```

| `MaHD` | Phép tính từ hai dòng chi tiết | `TongTien` |
| --- | --- | --- |
| 1 | `20 × 1.500 + 10 × 1.500` | 45.000 đồng |
| 2 | `15 × 2.000 + 8 × 2.500` | 50.000 đồng |
| 3 | `8 × 1.000 + 10 × 1.500` | 23.000 đồng |
| 4 | `6 × 1.200 + 5 × 5.000` | 32.200 đồng |
| 5 | `12 × 1.000 + 10 × 2.500` | 37.000 đồng |

Các tổng tiền trên được tính sẵn và ghi trực tiếp trong `VALUES`. Schema có ràng buộc số tiền hợp lệ, còn việc tổng phần đầu khớp tổng chi tiết là điều phải tính đúng khi chuẩn bị dữ liệu hoặc lưu giao dịch.

`CT_HoaDon.DonGia` lưu giá bán tại thời điểm lập hóa đơn. Khi giá hiện tại trong `Thuoc.GiaBan` thay đổi sau này, các dòng hóa đơn đã lưu vẫn có đơn giá của lần bán trước đó.

### Tồn kho cuối kỳ và quan hệ với giao dịch mẫu

Trong bộ seed, toàn bộ số lượng tồn được giải thích bằng các lần nhập và bán đã tạo. Công thức cho từng thuốc là

```text
Thuoc.SoLuongTon = tổng số lượng đã nhập - tổng số lượng đã bán
```

| `MaThuoc` | Tổng nhập | Tổng bán | `SoLuongTon` |
| --- | --- | --- | --- |
| 1 | 200 | 20 | 180 |
| 2 | 150 | 15 | 135 |
| 3 | 100 | 10 | 90 |
| 4 | 80 | 8 | 72 |
| 5 | 15 | 8 | 7 |
| 6 | 15 | 6 | 9 |
| 7 | 100 | 10 | 90 |
| 8 | 60 | 5 | 55 |
| 9 | 120 | 12 | 108 |
| 10 | 100 | 10 | 90 |

Ví dụ, Loratadin được nhập `15` viên và bán `8` viên, nên còn `7` viên. Con số này vừa khớp giao dịch vừa tạo tình huống sắp hết hàng.

Seed ghi trực tiếp số tồn cuối kỳ trong câu `INSERT INTO dbo.Thuoc`. Các lệnh chèn chi tiết phía sau bổ sung lịch sử tương ứng. Khi ứng dụng thực hiện giao dịch mới, luồng lưu phiếu nhập hoặc hóa đơn cần cập nhật số tồn cùng transaction với phần đầu và phần chi tiết.

### Transaction, `TRY/CATCH` và xử lý lỗi

Toàn bộ các lệnh chèn nằm trong cấu trúc sau

```sql
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Các lệnh INSERT vào 9 bảng.

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
```

**`SET NOCOUNT ON`** giảm các thông báo số dòng bị tác động sau từng lệnh. Các lệnh `INSERT` vẫn ghi đầy đủ dữ liệu.

**`BEGIN TRANSACTION`** bắt đầu một nhóm thay đổi cần được lưu cùng nhau. Với T06, nhóm này bao gồm toàn bộ bộ dữ liệu của 9 bảng. **`COMMIT TRANSACTION`** xác nhận lưu khi các lệnh đã thực hiện thành công.

**`SET XACT_ABORT ON`** yêu cầu SQL Server hủy transaction với các lỗi thực thi thuộc phạm vi hỗ trợ của thiết lập này. **`TRY/CATCH`** chuyển việc xử lý lỗi thực thi bắt được sang khối `CATCH`.

**`@@TRANCOUNT`** cho biết số mức transaction đang mở trên kết nối. Trong cách chạy độc lập của file seed, giá trị thường là `1` sau `BEGIN TRANSACTION`. Điều kiện `> 0` giúp chỉ gọi `ROLLBACK` khi còn transaction đang mở.

**`ROLLBACK TRANSACTION`** hoàn tác các thay đổi dữ liệu của transaction. **`THROW;`** ném lại lỗi ban đầu để người chạy đọc được thông báo và vị trí lỗi.

Ví dụ, nếu một dòng chi tiết dùng `MaThuoc` chưa tồn tại, khóa ngoại sẽ từ chối lệnh đó. Khi lỗi được bắt, khối `CATCH` hoàn tác các dòng đã chèn trước đó trong transaction, giúp bộ dữ liệu tránh trạng thái chỉ có một phần bảng được nạp.

SQL Server có thể đã cấp các giá trị `IDENTITY` trước khi transaction bị rollback. Những mã này có thể bị bỏ trống sau lỗi. Đây cũng là lý do cần hiểu giả định CSDL mới tạo khi sử dụng các mã cố định trong seed.

## 4. Luồng hoạt động

### Khởi tạo bộ dữ liệu T06

1. Thành viên chạy `database/01_schema.sql` trong SSMS. Script tạo lại `PharmacyDB` và định nghĩa 9 bảng cùng các khóa, kiểu dữ liệu, ràng buộc.
2. Thành viên chạy toàn bộ `database/02_seed.sql`. `USE PharmacyDB` chọn CSDL, các thiết lập đầu file được áp dụng và `@NgayHienTai` nhận ngày của SQL Server.
3. `BEGIN TRANSACTION` bắt đầu việc nạp bộ dữ liệu.
4. Script chèn loại thuốc, nhà cung cấp, khách hàng và nhân viên. SQL Server tự cấp các mã bắt đầu từ `1` trên các bảng mới.
5. Script chèn 10 thuốc, gắn mỗi thuốc với loại đã có, đồng thời ghi giá bán, tồn kho cuối kỳ và hạn dùng tính theo ngày chạy.
6. Script chèn 5 phiếu nhập và 10 dòng chi tiết, sau đó chèn 5 hóa đơn và 10 dòng chi tiết bán.
7. SQL Server kiểm tra kiểu dữ liệu, khóa chính, khóa ngoại, các giá trị UNIQUE và CHECK khi thực hiện từng lệnh.
8. Khi mọi lệnh thành công, `COMMIT` xác nhận lưu bộ dữ liệu. Nếu phát sinh lỗi thực thi được bắt, `CATCH` xử lý rollback và ném lại lỗi.

### Theo dõi một thuốc từ nhập đến tồn cuối kỳ

Lấy Panadol 500 mg làm ví dụ

```text
Loại thuốc 1 (Giảm đau, hạ sốt)
→ Thuốc 1 (Panadol 500 mg)
→ Phiếu nhập 1, chi tiết nhập 200 viên với giá 1.000 đồng/viên
→ Hóa đơn 1, chi tiết bán 20 viên với giá 1.500 đồng/viên
→ Tồn cuối kỳ 180 viên, hạn dùng sau 7 ngày tính từ lúc seed
```

Dòng Panadol đóng góp `200.000` đồng vào phiếu nhập `1` và `30.000` đồng vào hóa đơn `1`. Số tồn `180` khớp phép tính `200 - 20`. Hạn dùng cộng 7 ngày tạo trường hợp kiểm tra cảnh báo sắp hết hạn ngay sau khi khởi tạo.

### Dữ liệu seed đi đến giao diện WPF

Với màn Loại thuốc hiện có, luồng đọc dữ liệu được nối với T06 như sau

```text
02_seed.sql chèn 5 loại thuốc vào PharmacyDB
→ LoaiThuocViewModel gọi Load()
→ LoaiThuocRepository.GetAll() đọc bảng LoaiThuoc
→ ViewModel đưa các dòng vào ObservableCollection Items
→ DataGrid nhận dữ liệu qua Binding ItemsSource
```

Binding giúp View hiển thị các loại thuốc vừa đọc từ CSDL. Khi người dùng thao tác trên màn hình, các nút gọi Command của ViewModel để thực hiện nghiệp vụ tương ứng. Dữ liệu tài khoản, thuốc và giao dịch của T06 cũng cung cấp đầu vào cho các màn chức năng sử dụng chúng.

## 5. Câu hỏi vấn đáp có thể gặp

**T06 làm được những gì?**

T06 tạo bộ dữ liệu ban đầu cho 9 bảng, tổng cộng 60 dòng. Có tài khoản quản lý và nhân viên, 10 thuốc, giao dịch nhập/bán khớp tổng tiền và tồn kho, cùng dữ liệu sắp hết hạn và sắp hết hàng.

**Vì sao phải chạy `01_schema.sql` trước `02_seed.sql`?**

File `01_schema.sql` tạo CSDL, bảng và ràng buộc. Seed cần các bảng này và dựa vào mã `IDENTITY` bắt đầu từ `1` trên CSDL vừa tạo để gán khóa ngoại đúng.

**Chạy lại seed trên CSDL đã có dữ liệu thì sao?**

Các lệnh `INSERT` sẽ tiếp tục thêm dòng. Các tài khoản hoặc số điện thoại mẫu đã tồn tại có thể vi phạm UNIQUE, còn mã tự tăng đã thay đổi có thể làm sai giả định tham chiếu. Bộ seed được chuẩn bị cho lần khởi tạo ngay sau khi tạo lại schema.

**Vì sao loại thuốc được chèn trước thuốc?**

`Thuoc.MaLoai` là khóa ngoại tham chiếu `LoaiThuoc.MaLoai`. Chèn loại trước giúp mã loại đã tồn tại khi SQL Server kiểm tra dòng thuốc.

**Vì sao trong `INSERT` danh mục bỏ cột mã?**

Các mã đó được định nghĩa `IDENTITY(1,1)`. SQL Server tự cấp mã cho dòng mới, còn seed cung cấp các thuộc tính như tên, giá và số lượng.

**`N'...'` có tác dụng gì?**

Tiền tố `N` tạo chuỗi Unicode để các giá trị tiếng Việt như tên loại, đơn vị Viên và địa chỉ được giữ đúng dấu khi ghi vào cột `NVARCHAR`.

**Vì sao số điện thoại được viết trong dấu nháy?**

Cột số điện thoại lưu chuỗi `VARCHAR`. Viết dạng chuỗi giúp giữ số `0` đầu và phù hợp với kiểu dữ liệu của cột.

**Tài khoản admin đăng nhập bằng gì và có vai trò nào?**

Tên đăng nhập là `admin`, mật khẩu mẫu là `admin123`, `VaiTro = N'QuanLy'` và `TrangThai = 1`. Comment đầu script ghi lại thông tin này để cả nhóm sử dụng khi kiểm tra.

**Mật khẩu admin được lưu vào CSDL như thế nào?**

Mật khẩu được chuyển thành byte UTF-8, băm bằng SHA-256 rồi biểu diễn thành chuỗi hex viết hoa. Chuỗi 64 ký tự được lưu trong `MatKhauHash`.

**Vì sao hash SHA-256 có 64 ký tự?**

SHA-256 cho kết quả 256 bit, tương đương 32 byte. Mỗi byte viết thành hai ký tự hex, nên tổng cộng có 64 ký tự.

**Khi viết chức năng đăng nhập, cần khớp gì với seed?**

Code cần dùng cùng cách lấy byte UTF-8, băm SHA-256 và tạo chuỗi hex viết hoa. Sau đó so sánh với `MatKhauHash` của tên đăng nhập tương ứng và kiểm tra tài khoản có `TrangThai = 1`.

**Vì sao bốn nhân viên có cùng hash?**

Bốn tài khoản mẫu đều dùng mật khẩu `nhanvien123`. Cùng đầu vào và cùng cách băm sẽ cho cùng chuỗi hash.

**Tài khoản `nv04` dùng để kiểm tra trường hợp nào?**

Tài khoản này có `TrangThai = 0`, tạo tình huống kiểm tra đăng nhập bằng tài khoản đã khóa. Điều kiện đăng nhập cần xét trạng thái cùng với thông tin xác thực.

**Thuốc nào phục vụ kiểm tra sắp hết hạn và sắp hết hàng?**

Panadol, Hapacol và Smecta có hạn dùng lần lượt sau 7, 14 và 28 ngày tính từ lúc chạy seed. Loratadin và Cetirizin có tồn lần lượt 7 và 9, đều dưới 10.

**Vì sao dùng `DATEADD` thay cho một ngày cố định?**

`DATEADD` tính hạn dùng từ ngày chạy script. Thành viên khởi tạo ở thời điểm khác vẫn có các trường hợp sắp hết hạn để kiểm tra ngay.

**Hạn dùng có tự dời sang ngày mới sau mỗi lần mở ứng dụng không?**

Ngày hết hạn được tính và lưu tại thời điểm chèn dữ liệu. Các lần mở ứng dụng sau đọc lại ngày đã lưu, còn khoảng cách đến ngày hiện tại sẽ thay đổi theo thời gian.

**Vì sao có bảng đầu phiếu và bảng chi tiết?**

Bảng đầu phiếu lưu thông tin chung của một giao dịch. Bảng chi tiết lưu từng thuốc trong giao dịch, giúp một phiếu nhập hoặc hóa đơn chứa nhiều thuốc với số lượng và đơn giá riêng.

**Hai thuốc trong cùng hóa đơn có làm trùng khóa chính không?**

Khóa chính của `CT_HoaDon` là cặp `(MaHD, MaThuoc)`. Hai dòng cùng hóa đơn có mã thuốc khác nhau thì có cặp khóa khác nhau.

**Hóa đơn khách lẻ được biểu diễn thế nào?**

Hóa đơn `5` có `MaKH = NULL`. Schema cho phép cột này nhận `NULL` để lưu giao dịch của khách chưa có thông tin trong danh mục.

**Giá bán của Panadol trong seed là giá theo đơn vị nào?**

Panadol có đơn vị Viên và giá bán mẫu 1.500 đồng/viên. Dòng bán 20 viên có thành tiền 30.000 đồng. Các thuốc có đơn vị Gói được tính tương tự theo số gói.

**Làm sao giải thích tổng tiền hóa đơn `1`?**

Hóa đơn bán 20 viên Panadol với giá 1.500 đồng và 10 viên Amoxicillin với giá 1.500 đồng. Tổng tiền là `20 × 1.500 + 10 × 1.500 = 45.000` đồng.

**Làm sao chứng minh tồn kho khớp với giao dịch?**

Với mỗi thuốc, lấy tổng số lượng nhập trừ tổng số lượng bán rồi đối chiếu `SoLuongTon`. Ví dụ Loratadin nhập 15, bán 8, nên số tồn đã ghi là 7.

**Số tồn cuối kỳ trong seed được ghi ở đâu?**

Giá trị cuối kỳ được ghi trực tiếp trong `INSERT INTO dbo.Thuoc`. Các chi tiết nhập và bán được chuẩn bị tương ứng để giải thích đúng số tồn đó.

**Vì sao lưu `DonGia` ở chi tiết hóa đơn?**

Để giữ giá tại thời điểm bán. Khi `Thuoc.GiaBan` được cập nhật về sau, hóa đơn cũ vẫn có đơn giá đã dùng để tính tổng tiền của lần bán đó.

**Vì sao toàn bộ seed nằm trong một transaction?**

Các bảng liên quan bằng khóa ngoại và số liệu nghiệp vụ. Transaction giúp lưu cả bộ khi thành công, đồng thời cho phép hoàn tác các thay đổi dữ liệu khi một lỗi thực thi được bắt trong quá trình chèn.

**`SET NOCOUNT ON` có làm mất kết quả chèn không?**

Thiết lập này giảm thông báo số dòng bị tác động. Các lệnh chèn vẫn thực hiện và lưu dữ liệu theo transaction.

**`XACT_ABORT`, `ROLLBACK` và `THROW` có vai trò gì?**

`XACT_ABORT ON` hỗ trợ hủy transaction khi gặp lỗi thực thi thuộc phạm vi của thiết lập. `ROLLBACK` hoàn tác transaction đang mở, còn `THROW;` ném lại lỗi ban đầu để người chạy biết nguyên nhân.

**Vì sao kiểm tra `@@TRANCOUNT > 0` trước khi rollback?**

Vì cần có transaction đang mở để thực hiện `ROLLBACK`. Điều kiện này xử lý cả trường hợp SQL Server đã hủy transaction trước khi đi vào `CATCH`.

**Rollback có bảo đảm mã tự tăng trở lại như trước không?**

Các giá trị `IDENTITY` đã cấp có thể bị bỏ trống sau rollback. Seed vì vậy cần được hiểu theo điều kiện chạy trên CSDL vừa tạo, nơi các mã bắt đầu đúng thứ tự dự kiến.

**Dữ liệu T06 liên quan thế nào đến Binding và Command?**

Seed cung cấp dữ liệu trong SQL Server. Repository đọc dữ liệu cho ViewModel, ViewModel đưa vào danh sách để View hiển thị qua Binding. Các thao tác của người dùng trên màn hình được nối đến ViewModel bằng Command.
