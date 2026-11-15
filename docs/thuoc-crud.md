# Màn Thuốc

## 1. Mục tiêu

Xem, thêm, sửa và xóa thuốc trong danh mục. Form chọn loại thuốc và hạn dùng, đồng thời kiểm tra giá bán và số lượng tồn trước khi ghi vào cơ sở dữ liệu.

## 2. File đã làm

- `Models/Thuoc.cs` biểu diễn một thuốc và phát thông báo khi dữ liệu của dòng thay đổi.
- `Data/ThuocRepository.cs` đọc và ghi bảng thuốc bằng ADO.NET, lấy tên loại bằng `JOIN`.
- `ViewModels/ThuocViewModel.cs` giữ danh sách thuốc, danh sách loại, dữ liệu form, thông báo lỗi và các Command.
- `Views/ThuocView.xaml` khai báo bảng, form, bộ chọn loại, bộ chọn ngày và các Binding.
- `Views/ThuocView.xaml.cs` gọi `InitializeComponent` để khởi tạo giao diện.
- `ViewModels/MainViewModel.cs` giữ màn đang mở và cung cấp lệnh điều hướng.
- `MainWindow.xaml` khai báo nút menu, `ContentControl` và `DataTemplate` cho màn Thuốc.

## 3. Khái niệm

- `DataGrid.ItemsSource` bind vào `Items`. `ObservableCollection<Thuoc>` báo khi thêm hoặc bỏ dòng. `SetProperty` trên Model báo khi sửa ô của một dòng hiện có.
- `ComboBox.ItemsSource` dùng `DanhSachLoai` lấy từ `LoaiThuocRepository`. `DisplayMemberPath="TenLoai"` hiển thị tên. `SelectedValuePath="MaLoai"` và binding hai chiều của `SelectedValue` đưa mã loại về ViewModel.
- `DatePicker.SelectedDate` bind hai chiều vào `HanSuDung` kiểu `DateTime?`. Giá trị `null` biểu diễn chưa chọn ngày. Model dùng `DateTime` vì cột SQL `DATE` bắt buộc có giá trị.
- Ô giá và tồn kho bind vào chuỗi với `UpdateSourceTrigger=PropertyChanged`. ViewModel dùng `decimal.TryParse` và `int.TryParse` để đưa lỗi vào `ErrorMessage` và khóa nút **Lưu** qua `CanExecute`. Giá là số đồng nguyên dương, tồn kho là số nguyên không âm.
- Repository truyền dữ liệu qua tham số SQL. Khi sửa, câu `UPDATE` chỉ ghi nếu tồn kho vẫn khớp với giá trị lúc đọc. Điều này tránh ghi đè số lượng đã đổi do nhập hoặc bán hàng.

## 4. Luồng hoạt động

1. Bấm **Thuốc**. Lệnh điều hướng tạo `ThuocViewModel`, tải danh sách thuốc và loại. `DataTemplate` hiển thị View tương ứng.
2. Bấm **Thêm mới**, điền các trường, chọn loại và hạn dùng. Khi dữ liệu hợp lệ, **Lưu** gọi `Insert`, nhận mã từ SQL Server rồi thêm dòng vào `Items`.
3. Chọn một dòng để điền dữ liệu vào form. **Lưu** gọi `Update`, rồi cập nhật Model khi SQL ghi thành công.
4. **Xóa** gọi Repository theo mã thuốc. Khi SQL Server trả lỗi 547 do thuốc được tham chiếu, dòng vẫn hiển thị và thông báo cho biết thuốc đã có trong phiếu nhập hoặc hóa đơn.

## 5. Câu hỏi vấn đáp có thể gặp

**Tại sao form dùng chuỗi cho giá và tồn, còn Model dùng kiểu số?** Chuỗi giữ được cả nội dung gõ dở hoặc sai để ViewModel báo lỗi cụ thể. Model chỉ chứa giá trị đã qua kiểm tra.

**Khi chọn loại, giá trị nào được ghi xuống bảng Thuoc?** `MaLoai` là khóa ngoại. `TenLoai` dùng để hiển thị và được lấy bằng `JOIN`.

**ViewModel biết khi nào thêm hay sửa bằng cách nào?** `SelectedItem` rỗng thì thêm. Khi có dòng đang chọn thì sửa theo `MaThuoc`.
