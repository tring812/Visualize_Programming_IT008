# Màn Khách hàng

## 1. Mục tiêu

Xem, thêm, sửa, xóa và tìm khách hàng theo đầu số điện thoại. Repository cung cấp hàm tra đúng số để màn Bán hàng gọi lại.

## 2. File đã làm

- `Models/KhachHang.cs` biểu diễn một dòng dữ liệu và thông báo khi thuộc tính đổi.
- `Data/KhachHangRepository.cs` chứa các lệnh CRUD, tìm theo đầu số và tra cứu theo số đầy đủ.
- `ViewModels/KhachHangViewModel.cs` quản lý danh sách, dữ liệu form, bộ lọc, lỗi và các Command.
- `Views/KhachHangView.xaml` khai báo DataGrid, ô tìm, form và các Binding.
- `Views/KhachHangView.xaml.cs` gọi `InitializeComponent` để khởi tạo giao diện.
- `ViewModels/MainViewModel.cs` cung cấp Command mở màn Khách hàng từ menu.
- `MainWindow.xaml` đăng ký nút menu và `DataTemplate` cho màn Khách hàng.

## 3. Khái niệm

- DataGrid bind `ItemsSource` với `ObservableCollection<KhachHang>`. Binding `SelectedItem` điền dữ liệu vào form khi chọn dòng, còn `SetProperty` giúp các ô cập nhật khi sửa.
- `UpdateSourceTrigger=PropertyChanged` cập nhật ViewModel ngay khi gõ. `SaveCommand.CanExecute` khóa nút Lưu khi form không hợp lệ. `ErrorMessage` hiển thị lỗi trong View.
- Số điện thoại là chuỗi để giữ số 0 đầu. Form nhận từ 1 đến 20 chữ số, khớp độ dài cột `VARCHAR(20)`.
- Repository truyền đầu số qua tham số `@Sdt` trong `LIKE @Sdt + '%'`. `GetBySdt` dùng dấu bằng để tra số đầy đủ và trả `null` khi không tìm thấy.
- Ràng buộc `UNIQUE` tại SQL Server chặn số trùng khi thêm hoặc sửa. ViewModel chuyển lỗi 2627 hoặc 2601 thành thông báo **Số điện thoại đã có.**

## 4. Luồng hoạt động

1. Mở **Khách hàng** từ menu để xem danh sách.
2. Nhập đầu số và bấm **Tìm** hoặc Enter. Xóa đầu số rồi tìm lại để xem toàn bộ.
3. Bấm **Thêm mới** hoặc chọn dòng để sửa. **Lưu** gọi Repository và cập nhật danh sách khi ghi thành công. Dòng chỉ xuất hiện nếu số điện thoại còn khớp bộ lọc đang áp dụng.
4. Chọn dòng và bấm **Xóa**. Nếu hóa đơn đang tham chiếu khách hàng, SQL Server trả lỗi 547 và màn hình giữ lại dòng.

## 5. Câu hỏi vấn đáp có thể gặp

**Tìm theo đầu số khác gì tra cứu khi bán?** Tìm theo đầu số trả về nhiều khách hàng. `GetBySdt` dùng số đầy đủ và trả về tối đa một khách hàng nhờ ràng buộc `UNIQUE`.

**Vì sao dựa vào `UNIQUE` để chặn số trùng?** Danh sách có thể đang lọc hoặc dữ liệu có thể vừa được ghi từ máy khác. SQL Server kiểm tra toàn bộ dữ liệu ngay lúc ghi.

**Nút Lưu quyết định thêm hay sửa như thế nào?** ViewModel gọi `Insert` khi chưa chọn dòng và gọi `Update` theo `MaKH` khi đã chọn dòng.
