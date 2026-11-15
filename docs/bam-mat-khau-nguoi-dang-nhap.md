# Băm mật khẩu và lưu người đang đăng nhập

## 1. Mục tiêu

Cung cấp hàm băm mật khẩu SHA-256 và nơi lưu thông tin người đang đăng nhập để màn đăng nhập, phân quyền và lập hóa đơn dùng chung.

## 2. File đã làm

- `PharmacyManagement/Data/PasswordHelper.cs`: lớp tiện ích băm mật khẩu; không truy cập CSDL.
- `PharmacyManagement/Models/CurrentUser.cs`: lớp tĩnh giữ `MaNV`, `HoTen`, `VaiTro` và thuộc tính tính toán `LaQuanLy`.

## 3. Khái niệm

**SHA-256.** `HashPassword` chuyển mật khẩu thành byte UTF-8 bằng `Encoding.UTF8.GetBytes`, băm bằng `SHA256.HashData`, rồi đổi kết quả thành chuỗi hex in hoa bằng `Convert.ToHexString`. Kết quả gồm 32 byte, tương ứng 64 ký tự hex, vừa với cột `NhanVien.MatKhauHash CHAR(64)`.

**Băm khác mã hóa.** Băm là phép biến đổi một chiều; đăng nhập không giải mã mật khẩu đã lưu mà băm mật khẩu vừa nhập rồi so sánh hai chuỗi băm. Cùng một mật khẩu và cùng cách mã hóa UTF-8 phải cho cùng kết quả để khớp dữ liệu seed.

**Giữ nguyên mật khẩu.** Hàm không `Trim`, không đổi hoa thường; khoảng trắng và chữ hoa là một phần của mật khẩu. Giá trị `null` bị từ chối bằng `ArgumentNullException`. Việc kiểm tra ô mật khẩu trống thuộc về ViewModel của màn đăng nhập/đăng ký.

**Lớp tĩnh `CurrentUser`.** Các màn truy cập cùng một bộ thông tin mà không cần tạo đối tượng. Ban đầu `MaNV = 0`, `HoTen = ""`, `VaiTro = ""`. Lớp chỉ lưu mã, tên và vai trò trong bộ nhớ, không giữ mật khẩu hoặc chuỗi băm.

**Thuộc tính tính toán `LaQuanLy`.** Giá trị được tính mỗi lần đọc bằng `VaiTro == "QuanLy"`, khớp tên vai trò trong schema. Vai trò `NhanVien` hoặc chuỗi rỗng trả về `false`; không cần gán riêng một biến bool có thể bị lệch với `VaiTro`.

**Liên hệ MVVM.** Hai lớp này không tạo giao diện, Binding hay Command. Màn đăng nhập sẽ gọi chúng từ ViewModel/Repository. Nếu cần hiển thị tên hoặc ẩn menu, ViewModel cung cấp thuộc tính cho View bind và phát `PropertyChanged` khi cập nhật; các thuộc tính tĩnh ở đây không tự thông báo cho giao diện.

## 4. Luồng hoạt động

Khi tích hợp màn đăng nhập:

1. ViewModel nhận tên đăng nhập và mật khẩu, kiểm tra dữ liệu nhập.
2. Gọi `PasswordHelper.HashPassword(matKhau)` để có chuỗi băm.
3. Repository kiểm tra tài khoản đang hoạt động và so sánh với `MatKhauHash` trong CSDL; câu SQL vẫn nằm trong Repository.
4. Chỉ sau khi xác thực thành công, gán đủ `CurrentUser.MaNV`, `CurrentUser.HoTen`, `CurrentUser.VaiTro` từ tài khoản vừa đọc.
5. Màn chính đọc `CurrentUser.LaQuanLy` để quyết định quyền; khi lập hóa đơn/phiếu nhập, dùng `CurrentUser.MaNV` để biết người lập.
6. Khi đăng xuất, màn phụ trách đặt lại `MaNV = 0`, `HoTen = ""`, `VaiTro = ""` và thông báo giao diện cập nhật. `LaQuanLy` sau đó trả về `false`.

## 5. Câu hỏi vấn đáp có thể gặp

**Vì sao chuỗi băm dài 64 ký tự?**
SHA-256 tạo 256 bit, tức 32 byte; mỗi byte được biểu diễn bằng hai ký tự hex nên có 64 ký tự.

**Vì sao dùng UTF-8 trước khi băm?**
Hàm băm nhận byte. UTF-8 xác định cách chuyển chuỗi, kể cả tiếng Việt, thành byte nhất quán trên các máy.

**Vì sao không cắt khoảng trắng hoặc đổi mật khẩu thành chữ thường?**
Làm vậy sẽ thay đổi mật khẩu người dùng nhập và khiến hai mật khẩu khác nhau có thể bị xử lý như nhau.

**Vì sao `CurrentUser` là static?**
Ứng dụng desktop này có một người đăng nhập tại một thời điểm; các màn cần đọc cùng mã nhân viên, họ tên và vai trò trong suốt lần chạy.

**`LaQuanLy` có lưu riêng trong bộ nhớ không?**
Không có biến lưu riêng. Getter kiểm tra `VaiTro` hiện tại mỗi lần được gọi, nên đổi vai trò thì kết quả lần đọc tiếp theo cũng đổi.

**Vì sao không lưu mật khẩu trong `CurrentUser`?**
Các màn chỉ cần thông tin nhận diện và vai trò sau khi xác thực, không cần giữ lại mật khẩu hay chuỗi băm.

**Chỉ ẩn menu quản lý đã đủ phân quyền chưa?**
Màn thực hiện thao tác quản lý cũng cần kiểm tra quyền; `LaQuanLy` là dữ liệu để các màn dùng, bản thân lớp này không thực thi hay chặn thao tác.
