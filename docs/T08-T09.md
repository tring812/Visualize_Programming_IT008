# T08 và T09 — nền tảng MVVM và kết nối SQL

## File triển khai

- `PharmacyManagement/ViewModels/BaseViewModel.cs`: `INotifyPropertyChanged`, `OnPropertyChanged`, `SetProperty<T>`.
- `PharmacyManagement/ViewModels/RelayCommand.cs`: `ICommand`, delegate thực thi và điều kiện thực thi.
- `PharmacyManagement/Data/DbHelper.cs`: nơi duy nhất giữ chuỗi kết nối, trả về `SqlConnection` đã mở.

Các lớp bám mẫu trong `.agents/skills/pharmacy-mvvm-screen/references/code-templates.md`.
`SetProperty` dùng `EqualityComparer<T>.Default`; constructor của Command chặn delegate null;
`DbHelper` giải phóng kết nối nếu mở lỗi và ném lại nguyên ngoại lệ bằng `throw;`.
ViewModel bắt `SqlException` từ Repository để đặt `ErrorMessage`, không gọi `MessageBox`.
Bên gọi dùng `using var connection = DbHelper.GetConnection();` để trả kết nối về pool sau khi dùng.

## Kiểm tra thủ công

Từ thư mục gốc repository:

```powershell
dotnet build PharmacyManagement.sln
```

1. T08: tạo tạm một ViewModel kế thừa `BaseViewModel`, có thuộc tính `Ten` dùng `SetProperty`.
   Bind `TextBox.Text` hai chiều vào `Ten` với `UpdateSourceTrigger=PropertyChanged`,
   và bind `TextBlock.Text` vào cùng thuộc tính. Gõ vào ô nhập: chữ hiển thị phải đổi theo.
2. Bind một nút vào `RelayCommand`, cho `CanExecute` kiểm tra `Ten` không trống.
   Xóa hết tên: nút bị khóa; nhập lại tên: nút được bật và thực thi lệnh khi bấm.
3. T09: xác nhận dịch vụ `SQL Server (SQLEXPRESS)` đang chạy và đã tạo `PharmacyDB`.
   Dùng tạm một Repository trong `Data/` gọi `DbHelper.GetConnection()` bằng `using`,
   kiểm tra trạng thái `Open` và chạy `SELECT 1` bằng `ExecuteScalar()`; kết quả phải là `1`.
4. Xóa các file và lời gọi thử trước khi commit. Hai task này chỉ cung cấp lớp nền tảng,
   không thêm project test hoặc giao diện thử vào solution.

Nếu lỗi network/instance: kiểm tra dịch vụ và tên instance trong SSMS.
Nếu lỗi không mở được `PharmacyDB`: chạy script tạo CSDL của task T05 trên đúng instance.
`SELECT 1` không cần bảng hay dữ liệu seed, nhưng `PharmacyDB` vẫn phải tồn tại.

## Giải thích khi vấn đáp

1. `INotifyPropertyChanged` cho Binding biết thuộc tính nguồn đã đổi. Setter gọi `SetProperty`
   để so sánh, gán và phát `PropertyChanged`; `[CallerMemberName]` tự lấy tên thuộc tính gọi setter.
2. `TextBox.Text` bind hai chiều vào `Ten`, với `UpdateSourceTrigger=PropertyChanged` để cập nhật
   ViewModel ngay khi gõ. `TextBlock.Text` bind vào cùng `Ten` để hiển thị giá trị mới.
3. `ICommand.Execute` chạy công việc; `CanExecute` quyết định nút có bấm được hay không.
   WPF kiểm tra `CanExecute` trước khi gọi `Execute`; khi gọi trực tiếp từ C# thì bên gọi tự kiểm tra.
4. `CanExecuteChanged` nối với `CommandManager.RequerySuggested`, giúp WPF hỏi lại điều kiện
   khi người dùng thao tác. Thay đổi từ code nền có thể cần `CommandManager.InvalidateRequerySuggested()`
   trên UI thread để yêu cầu cập nhật nút.
5. `SqlConnection` là kết nối, `SqlCommand` là câu lệnh; `ExecuteScalar()` lấy ô đầu tiên của kết quả.
   `DbHelper` chỉ mở kết nối, câu SQL nghiệp vụ nằm trong Repository. Lỗi SQL giữ kiểu `SqlException`
   để ViewModel phân biệt lỗi bằng `Number`. `ObservableCollection` sẽ dùng ở các màn danh sách
   để thông báo thêm/xóa phần tử; hai task nền tảng này chưa cần danh sách.

## Phối hợp nhóm

Ba lớp trên là file chung: báo nhóm khi chuẩn bị gộp để các task sau dùng cùng API.
Kết quả build và chạy thử ghi trong mô tả PR, không lưu nhật ký môi trường vào tài liệu này.
T08/T09 ở trạng thái **Chờ review** trong lúc chờ kiểm tra và duyệt;
chỉ chuyển **Xong** sau khi kiểm tra SQL thành công và trưởng nhóm gộp PR.
