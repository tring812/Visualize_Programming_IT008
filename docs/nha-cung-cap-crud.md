# Màn Nhà cung cấp

## 1. Mục tiêu

Xem, thêm, sửa và xóa nhà cung cấp để dùng cho phiếu nhập. Khi đã có phiếu nhập tham chiếu, màn hình giữ lại nhà cung cấp và giải thích lý do.

## 2. File đã làm

- `Models/NhaCungCap.cs` biểu diễn một nhà cung cấp và báo thay đổi thuộc tính.
- `Data/NhaCungCapRepository.cs` đọc và ghi bảng `dbo.NhaCungCap` bằng ADO.NET.
- `ViewModels/NhaCungCapViewModel.cs` quản lý danh sách, dòng chọn, dữ liệu form và các lệnh.
- `Views/NhaCungCapView.xaml` khai báo bảng, form, thông báo lỗi và nút thao tác.
- `Views/NhaCungCapView.xaml.cs` khởi tạo các thành phần XAML.
- `ViewModels/MainViewModel.cs` quản lý màn hình đang mở và lệnh điều hướng.
- `MainWindow.xaml` đặt menu và ghép ViewModel với View qua `DataTemplate`.

## 3. Khái niệm

- Binding của các ô nhập cập nhật ViewModel ngay khi gõ nhờ `UpdateSourceTrigger=PropertyChanged`. Chọn một dòng qua `SelectedItem` sẽ điền dữ liệu vào form.
- `SaveCommand.CanExecute` chỉ cho lưu khi tên hợp lệ, số điện thoại khớp mẫu `\A[0-9]{10}\z` và địa chỉ nằm trong giới hạn cột. Giữ số điện thoại dưới dạng chuỗi để giữ số 0 đầu.
- `ObservableCollection` báo thêm hoặc bỏ dòng. Khi sửa, Model dùng `SetProperty` để DataGrid cập nhật các ô tương ứng.
- Repository dùng tham số có kiểu khớp schema. Địa chỉ rỗng được lưu thành SQL `NULL`. `OUTPUT INSERTED.MaNCC` lấy mã mà SQL Server vừa sinh.
- `CurrentViewModel` là nội dung của `ContentControl`. WPF chọn `DataTemplate` theo kiểu ViewModel.

## 4. Luồng hoạt động

1. Bấm **Nhà cung cấp**. Command đổi `CurrentViewModel`. ViewModel đọc dữ liệu qua Repository và WPF hiển thị danh sách.
2. Bấm **Thêm mới**, điền tên và số điện thoại 10 chữ số, rồi bấm **Lưu**. Repository thêm bản ghi, trả mã mới và ViewModel thêm dòng vào danh sách.
3. Chọn một dòng, sửa dữ liệu rồi bấm **Lưu**. Sau khi cập nhật SQL thành công, ViewModel gán lại dữ liệu lên dòng đã chọn.
4. Chọn dòng và bấm **Xóa**. Nếu SQL Server trả lỗi khóa ngoại 547 vì đã có phiếu nhập, ViewModel giữ dòng và hiện thông báo qua `ErrorMessage`.

## 5. Câu hỏi vấn đáp có thể gặp

**Làm sao nút Lưu biết cần thêm hay sửa?** Khi `SelectedItem` rỗng thì gọi `Insert`. Khi đã chọn dòng thì gọi `Update` theo `MaNCC`.

**Tại sao dùng tham số SQL?** SQL Server nhận dữ liệu riêng với câu lệnh, nên tên có dấu nháy vẫn được lưu đúng.

**Tại sao bắt lỗi 547 sau khi gọi xóa?** Khóa ngoại trong SQL Server kiểm tra quan hệ ngay khi thực hiện lệnh. ViewModel dùng mã lỗi để hiện lý do và giữ nguyên dòng trên màn hình.
