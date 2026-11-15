# Màn Loại thuốc (màn CRUD mẫu)

## 1. Mục tiêu

Làm màn **Loại thuốc** có đủ xem, thêm, sửa, xóa theo đúng MVVM. Đây là màn mẫu: các màn CRUD khác (Thuốc, Nhà cung cấp, Khách hàng, Nhân viên) sao chép đúng khuôn 4 lớp này.

## 2. File đã làm

| File | Lớp | Vai trò |
| --- | --- | --- |
| `PharmacyManagement/Models/LoaiThuoc.cs` | Model | Một dòng của bảng `LoaiThuoc` (`MaLoai`, `TenLoai`), báo giao diện khi đổi tên |
| `PharmacyManagement/Data/LoaiThuocRepository.cs` | Repository | Bốn hàm `GetAll`, `Insert`, `Update`, `Delete`; nơi duy nhất có câu SQL |
| `PharmacyManagement/ViewModels/LoaiThuocViewModel.cs` | ViewModel | Danh sách `Items`, `SelectedItem`, ô nhập `TenLoai`, `ErrorMessage`, ba lệnh `AddCommand`, `SaveCommand`, `DeleteCommand` |
| `PharmacyManagement/Views/LoaiThuocView.xaml` (+ `.xaml.cs`) | View | `UserControl`: DataGrid + ô nhập + dòng lỗi đỏ + 3 nút, chỉ có Binding |
| `PharmacyManagement/MainWindow.xaml` | View (file chung) | `DataTemplate` nối `LoaiThuocViewModel` với `LoaiThuocView`; tạm hiển thị thẳng màn này cho tới khi T10 làm menu |

## 3. Khái niệm

**Model kế thừa `BaseViewModel`.** Setter của `TenLoai` gọi `SetProperty`, nên khi ViewModel gán `SelectedItem.TenLoai = ten` sau khi sửa, ô trên DataGrid tự đổi chữ mà không phải nạp lại danh sách.

**Binding hai chiều.** `TextBox.Text="{Binding TenLoai, UpdateSourceTrigger=PropertyChanged}"`: người dùng gõ thì thuộc tính `TenLoai` của ViewModel đổi ngay từng ký tự; ViewModel gán `TenLoai = ""` thì ô nhập trống theo. Không có `UpdateSourceTrigger=PropertyChanged` thì giá trị chỉ được đẩy về khi rời khỏi ô, nút Lưu sẽ không sáng lên khi đang gõ.

**`ObservableCollection<LoaiThuoc> Items`.** DataGrid bind `ItemsSource="{Binding Items}"`. Khi ViewModel gọi `Items.Add(...)` hay `Items.Remove(...)`, collection phát sự kiện `CollectionChanged` và DataGrid thêm/bớt dòng. Nếu dùng `List<T>` thì danh sách đổi nhưng giao diện không biết.

**`SelectedItem`.** DataGrid bind `SelectedItem="{Binding SelectedItem}"` (mặc định hai chiều). Bấm một dòng → setter chạy → đổ `TenLoai` xuống form. ViewModel gán `SelectedItem = null` → DataGrid bỏ chọn. `SelectedItem` cũng quyết định Lưu là thêm hay sửa: `null` là thêm mới, khác `null` là sửa dòng đó.

**Command và `CanExecute`.** Nút dùng `Command="{Binding SaveCommand}"` thay cho sự kiện `Click`. `SaveCommand = new RelayCommand(_ => Save(), _ => IsValid())`: `IsValid()` trả `false` khi tên trống hoặc toàn khoảng trắng, WPF tự làm mờ nút Lưu. `DeleteCommand` chỉ sáng khi đã chọn dòng (`SelectedItem != null`). WPF hỏi lại `CanExecute` mỗi lần người dùng gõ phím hay bấm chuột nhờ `CommandManager.RequerySuggested` trong `RelayCommand`.

**`ErrorMessage`.** Một chuỗi bind vào `TextBlock` màu đỏ. Mọi lỗi (tên quá dài, trùng tên, xóa loại đang có thuốc, mất kết nối) đều gán vào đây, ViewModel không gọi `MessageBox` và không đụng tới control nào.

**Tham số `@ten`.** `INSERT INTO LoaiThuoc (TenLoai) ... VALUES (@TenLoai)` rồi `cmd.Parameters.AddWithValue("@TenLoai", item.TenLoai)`. SQL Server nhận giá trị riêng với câu lệnh, nên tên có dấu nháy như `Thuốc d'Ân` không làm hỏng câu SQL và không bị SQL injection.

**`OUTPUT INSERTED.MaLoai` + `ExecuteScalar`.** `MaLoai` là cột `IDENTITY`, SQL Server tự sinh. `OUTPUT INSERTED.MaLoai` trả về mã vừa sinh, `ExecuteScalar()` đọc ô đó để gán cho đối tượng mới trước khi thêm vào `Items`.

**`using var conn = DbHelper.GetConnection()`.** Mỗi hàm Repository mở kết nối qua `DbHelper` (chuỗi kết nối chỉ nằm ở đó), `using` bảo đảm kết nối được đóng khi hàm kết thúc, kể cả khi có lỗi.

**Bắt `SqlException` theo `Number`.** Repository để lỗi nổi lên; ViewModel bắt trong `try/catch`:
- `547` (vi phạm khóa ngoại `FK_Thuoc_LoaiThuoc`) khi xóa loại đang có thuốc → "Không xóa được vì loại này đang có thuốc."
- `2627`/`2601` (trùng khóa UNIQUE) → "Tên loại này đã tồn tại." (để sẵn cho trường hợp nhóm thêm UNIQUE sau).
- Lỗi khác → câu báo chung "Lưu/Xóa không thành công".

**Kiểm tra trùng tên trong ViewModel.** Bảng `LoaiThuoc` trong `01_schema.sql` không có ràng buộc UNIQUE trên `TenLoai`, nên `Save()` dò trong `Items` một dòng khác dòng đang sửa có cùng tên (không phân biệt hoa thường). Bỏ qua chính dòng đang sửa để bấm Lưu mà giữ nguyên tên vẫn được.

**`DataTemplate` theo kiểu dữ liệu.** Trong `MainWindow.xaml`, `<DataTemplate DataType="{x:Type vm:LoaiThuocViewModel}"><views:LoaiThuocView/></DataTemplate>` nghĩa là: hễ `ContentControl` chứa một `LoaiThuocViewModel` thì WPF vẽ bằng `LoaiThuocView` và gán ViewModel đó làm `DataContext` của View. Nhờ vậy View không tự `new` ViewModel. T10 sẽ dùng đúng cơ chế này với `CurrentViewModel`.

## 4. Luồng hoạt động

### Mở màn

`MainWindow` tạo `LoaiThuocViewModel` → constructor tạo 3 lệnh rồi gọi `Load()` → `_repo.GetAll()` chạy `SELECT MaLoai, TenLoai FROM LoaiThuoc ORDER BY TenLoai` → mỗi dòng đọc được `Items.Add(...)` → DataGrid hiện danh sách. Không kết nối được thì `ErrorMessage` báo lỗi kết nối.

### Bấm Lưu (thêm mới)

1. Người dùng gõ "Kháng sinh" vào ô Tên loại → Binding gán `TenLoai = "Kháng sinh"` từng ký tự.
2. WPF hỏi `SaveCommand.CanExecute` → `IsValid()` trả `true` → nút Lưu sáng.
3. Bấm Lưu → `SaveCommand.Execute` → `Save()`.
4. `Save()` cắt khoảng trắng (`Trim`), kiểm tra không quá 100 ký tự, kiểm tra không trùng tên trong `Items`. Sai thì gán `ErrorMessage` và dừng.
5. `SelectedItem == null` nên là thêm mới: tạo `LoaiThuoc`, gọi `_repo.Insert(item)`.
6. `Insert` mở kết nối, chạy `INSERT ... OUTPUT INSERTED.MaLoai VALUES (@TenLoai)`, trả về mã mới.
7. ViewModel gán `item.MaLoai`, gọi `Items.Add(item)` → DataGrid có thêm dòng.
8. `ClearForm()` gán `SelectedItem = null`, `TenLoai = ""`, `ErrorMessage = ""` → form trống, nút Lưu mờ lại.

### Bấm Lưu (sửa)

Bấm một dòng → setter `SelectedItem` đổ tên xuống form → sửa chữ → Lưu → `Save()` thấy `SelectedItem != null` → `_repo.Update(...)` chạy `UPDATE ... WHERE MaLoai = @MaLoai` → thành công mới gán `SelectedItem.TenLoai = ten` → Model báo `PropertyChanged` → ô trên DataGrid đổi chữ → `ClearForm()`.

### Bấm Xóa loại đang có thuốc

Chọn dòng → nút Xóa sáng → bấm → `Delete()` → `_repo.Delete(maLoai)` chạy `DELETE ... WHERE MaLoai = @MaLoai` → SQL Server từ chối vì còn thuốc tham chiếu → `SqlException` số 547 → ViewModel bắt và gán `ErrorMessage = "Không xóa được vì loại này đang có thuốc."` → dòng chữ đỏ hiện ra, `Items` giữ nguyên.

## 5. Câu hỏi vấn đáp có thể gặp

**Vì sao nút Lưu tự mờ khi ô tên trống?**
`SaveCommand` có `CanExecute = IsValid()`. Nút bind vào Command nên WPF gọi `CanExecute`; trả `false` thì nút bị khóa. `UpdateSourceTrigger=PropertyChanged` làm `TenLoai` cập nhật ngay khi gõ nên nút đổi trạng thái tức thì.

**Vì sao dùng `ObservableCollection` mà không dùng `List`?**
`ObservableCollection` phát `CollectionChanged` khi thêm/xóa, DataGrid nghe sự kiện này để vẽ lại. `List` không có sự kiện đó nên lưới không đổi.

**Sửa tên xong, sao DataGrid đổi mà không cần nạp lại?**
`LoaiThuoc` kế thừa `BaseViewModel`; setter `TenLoai` gọi `SetProperty` phát `PropertyChanged`, cột "Tên loại" bind vào `TenLoai` nên tự cập nhật.

**Làm sao biết Lưu là thêm hay sửa?**
Dựa vào `SelectedItem`: `null` (vừa bấm Thêm mới hoặc chưa chọn dòng) là `INSERT`, có dòng đang chọn là `UPDATE` dòng đó.

**Sao không gọi `MessageBox` để báo lỗi?**
ViewModel không được phụ thuộc giao diện để có thể chạy và kiểm thử mà không cần cửa sổ. Lỗi được đưa vào thuộc tính `ErrorMessage`, View tự hiển thị qua Binding.

**Sao không nối chuỗi `"... VALUES (N'" + ten + "')"`?**
Nối chuỗi bị SQL injection và vỡ câu lệnh khi tên có dấu nháy. Tham số `@TenLoai` gửi giá trị tách khỏi câu lệnh.

**Xóa loại đang có thuốc thì chuyện gì xảy ra?**
Khóa ngoại `FK_Thuoc_LoaiThuoc` chặn lệnh `DELETE`, SQL Server ném `SqlException` số 547. ViewModel bắt đúng số này và báo "Không xóa được vì loại này đang có thuốc.".

**Kiểm tra trùng tên ở đâu, vì sao?**
Trong `Save()` của ViewModel, dò trong `Items`, vì bảng chưa có UNIQUE trên `TenLoai`. Vẫn giữ `catch` số 2627/2601 để nếu nhóm thêm UNIQUE thì lỗi từ SQL cũng được báo đúng câu.

**View lấy ViewModel ở đâu, sao code-behind trống?**
`MainWindow` đặt một `LoaiThuocViewModel` vào `ContentControl`; `DataTemplate` theo kiểu cho WPF biết vẽ nó bằng `LoaiThuocView` và gán làm `DataContext`. Code-behind chỉ còn `InitializeComponent()`.

**Vì sao ghi CSDL trước rồi mới sửa `Items`?**
Nếu SQL lỗi thì `Items` không bị đổi, giao diện luôn khớp với dữ liệu thật trong CSDL.
