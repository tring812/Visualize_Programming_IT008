---
name: pharmacy-review-checklist
description: Checklist tự kiểm tra code của đồ án Quản lý tiệm thuốc theo bảng chấm điểm IT008 (chức năng, giao diện, Data binding/Command/MVVM, cấu trúc mã nguồn) trước khi tạo Pull Request hoặc khi review PR của bạn cùng nhóm. Dùng skill này mỗi khi người dùng nói "review", "kiểm tra code", "trước khi tạo PR", "đã đúng MVVM chưa", "có mất điểm không", "check lại", "rà soát", "chuẩn bị demo", hoặc vừa làm xong một màn hình.
---

# Checklist trước khi tạo Pull Request

Bảng chấm IT008 (10 điểm): Chức năng 2 · Giao diện 2 · Data binding, Command, MVVM 3 · Cấu trúc mã nguồn 1 · Teamwork 0.5 · Demo và trả lời 1.5. Mỗi mục dưới đây gắn với một tiêu chí, để khi báo lỗi có thể nói rõ lỗi đó làm mất điểm ở đâu.

## Cách chạy

1. Xác định phạm vi: các file thay đổi so với `main` (`git diff --name-only main...HEAD`), hoặc file người dùng chỉ định.
2. Chạy các lệnh dò nhanh bên dưới trên phạm vi đó. Chúng chỉ là gợi ý: đọc từng kết quả để loại các trường hợp hợp lệ trước khi báo lỗi.
3. Đọc code theo từng nhóm mục.
4. Báo kết quả theo mẫu ở cuối, xếp lỗi nặng nhất lên đầu. Khi người dùng đồng ý thì mới sửa.

## Lệnh dò nhanh

```bash
# Sự kiện Click trong XAML (nên là Command)
git grep -n 'Click="' -- '*.xaml'
# MessageBox hoặc control trong ViewModel
git grep -nE 'MessageBox|System\.Windows\.Controls' -- '*ViewModels/*.cs'
# SQL nối chuỗi (nên dùng tham số @ten)
git grep -nE '"(SELECT|INSERT|UPDATE|DELETE)[^"]*"\s*\+|\$"(SELECT|INSERT|UPDATE|DELETE)' -- '*.cs'
# SQL nằm ngoài Repository
git grep -nlE 'SqlCommand|SqlConnection' -- '*.cs' ':!*Data/*'
# List thay cho ObservableCollection trong ViewModel
git grep -nE 'public\s+List<' -- '*ViewModels/*.cs'
# Kích thước cứng trên vùng chứa
git grep -nE '<(Grid|DockPanel|StackPanel|DataGrid)[^>]*\s(Width|Height)="[0-9]+' -- '*.xaml'
# Chuỗi kết nối nằm ngoài DbHelper
git grep -n 'SQLEXPRESS' -- '*.cs' ':!*DbHelper.cs'
```

Trong pathspec của git, `*` khớp cả dấu `/`, nên `*ViewModels/*.cs` tìm được cả khi code nằm trong thư mục con `PharmacyManagement/`. Nếu nhóm đặt tên thư mục khác, đổi lại cho khớp.

## 1. MVVM, Binding, Command (3 điểm)

- [ ] View gán `DataContext` là ViewModel (qua `DataTemplate` của MainWindow hoặc trong XAML), không tạo dữ liệu trong code-behind.
- [ ] Code-behind chỉ còn `InitializeComponent()` và việc thuần giao diện.
- [ ] Mọi nút dùng `Command`, không có `Click=`.
- [ ] Danh sách là `ObservableCollection<T>`; setter gọi `OnPropertyChanged`.
- [ ] `SaveCommand` có `CanExecute` khóa nút khi dữ liệu chưa hợp lệ; ô nhập dùng `UpdateSourceTrigger=PropertyChanged`.
- [ ] ViewModel không gọi `MessageBox`, không tham chiếu control (ngoại lệ đã thống nhất: `PasswordBox` qua `CommandParameter`).

## 2. Chức năng và độ ổn định (2 điểm)

- [ ] Thêm, sửa, xóa, xem đều chạy; danh sách cập nhật ngay không cần mở lại màn.
- [ ] Kiểm tra nhập liệu: ô trống, số âm, chữ trong ô số, ngày sai, chuỗi quá dài. Lỗi hiện ở `ErrorMessage` bằng câu dễ hiểu.
- [ ] Mọi lời gọi Repository trong `try/catch`; lỗi khóa ngoại (547) và trùng (2627/2601) có câu riêng. Tắt SQL Server rồi mở màn: ứng dụng báo lỗi, không văng.
- [ ] Bán hàng: chặn bán quá tồn và thuốc hết hạn; lưu bằng transaction (xem skill `pharmacy-database`).
- [ ] Phân quyền: đăng nhập bằng Nhân viên không thấy menu chỉ dành cho Quản lý.

## 3. Giao diện (2 điểm)

- [ ] Kéo giãn cửa sổ từ nhỏ đến toàn màn hình: không bị cắt, không chồng chữ. Dùng `*` và `Auto`.
- [ ] Style lấy từ `Resources/Styles.xaml`; không đặt màu, font rời rạc từng control.
- [ ] Nhãn tiếng Việt có dấu, nhất quán giữa các màn (cùng thứ tự nút Thêm mới / Lưu / Xóa).
- [ ] DataGrid có tiêu đề cột rõ, không tự sinh cột (`AutoGenerateColumns="False"`).

## 4. Cấu trúc mã nguồn (1 điểm)

- [ ] File đúng thư mục: `Models/`, `Data/`, `ViewModels/`, `Views/`, `Resources/`.
- [ ] Tên lớp theo quy ước: `ThuocViewModel`, `ThuocRepository`, `ThuocView`; Command kết thúc bằng `Command`.
- [ ] SQL chỉ nằm trong Repository, luôn dùng tham số `@ten`.
- [ ] Chuỗi kết nối chỉ ở `DbHelper.cs`.
- [ ] Không còn code chết, `Console.WriteLine` gỡ lỗi, khối comment code cũ.
- [ ] Không commit `bin/`, `obj/`, `.vs/`.

## 5. Teamwork và demo

- [ ] Commit message có mã task; PR mô tả cách thử.
- [ ] Nếu sửa `database/*.sql`: cả hai script chạy lại được trên máy sạch, và PR ghi "cần chạy lại script".
- [ ] Người viết tự giải thích được: Binding nào nối thuộc tính nào, Command nào làm gì, vì sao dùng `ObservableCollection`. Nếu có chỗ họ chưa giải thích được, chỉ ra để họ đọc lại trước buổi demo.

## Mẫu báo cáo

```markdown
## Kết quả review — <mã task hoặc nhánh>

**Cần sửa trước khi gộp**
1. [MVVM −3đ] Views/ThuocView.xaml:42 — nút Lưu dùng Click="BtnLuu_Click". Đổi sang Command="{Binding SaveCommand}".
2. ...

**Nên sửa**
- [Giao diện] ...

**Đã đạt**
- Danh sách dùng ObservableCollection, SQL có tham số, ...
```

Mỗi lỗi ghi `file:dòng`, tiêu chí bị ảnh hưởng, và cách sửa một dòng. Không liệt kê mục đã đạt quá dài, chỉ đủ để người viết biết phần nào ổn.
