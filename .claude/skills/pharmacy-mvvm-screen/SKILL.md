---
name: pharmacy-mvvm-screen
description: Tạo hoặc sửa một màn hình WPF trong đồ án Quản lý tiệm thuốc theo đúng khuôn MVVM của nhóm (Model + Repository ADO.NET + ViewModel + View XAML). Dùng skill này mỗi khi người dùng muốn làm, thêm, sửa hay debug bất kỳ màn hình nào (Loại thuốc, Thuốc, Nhà cung cấp, Khách hàng, Nhân viên, Nhập hàng, Bán hàng, Tồn kho, Đăng nhập, Đăng ký, MainWindow), viết ViewModel, RelayCommand, Binding, DataGrid, ObservableCollection, hoặc nói "làm màn", "CRUD", "form nhập", "screen", "view", "viewmodel" — kể cả khi không nhắc chữ MVVM.
---

# Màn hình WPF theo MVVM (đồ án Quản lý tiệm thuốc)

Tiêu chí "Data binding, Command, MVVM" chiếm 3/10 điểm, nhiều nhất bảng chấm. Vì vậy mọi màn hình đều đi qua đúng ba lớp, và code phải đơn giản tới mức sinh viên viết ra nó giải thích lại được khi vấn đáp.

```
View (XAML)  --Binding/Command-->  ViewModel  --gọi hàm-->  Repository  --SQL-->  SQL Server
```

## Trước khi viết code

1. Đọc `README.md` ở gốc repo để nắm quy ước chung.
2. Xem màn mẫu **Loại thuốc** (`Views/LoaiThuocView.xaml`, `ViewModels/LoaiThuocViewModel.cs`, `Data/LoaiThuocRepository.cs`) nếu đã có. Màn mới sao chép đúng khuôn đó, vì cả nhóm thống nhất một kiểu để dễ review và dễ gộp code.
3. Nếu chưa có `BaseViewModel`, `RelayCommand`, `DbHelper`, lấy mẫu trong [references/code-templates.md](references/code-templates.md). Không tự viết lại bản khác khi bản của nhóm đã tồn tại.

## Các file cần tạo cho một màn hình `X`

| Thư mục | File | Nội dung |
| --- | --- | --- |
| `Models/` | `X.cs` | Lớp C# thuần, một lớp ứng một bảng, cài `INotifyPropertyChanged` (kế thừa `BaseViewModel` cũng được) |
| `Data/` | `XRepository.cs` | `GetAll()`, `Insert()`, `Update()`, `Delete()`; nơi duy nhất chứa SQL |
| `ViewModels/` | `XViewModel.cs` | `ObservableCollection<X> Items`, `SelectedItem`, các thuộc tính form, `ErrorMessage`, `AddCommand`, `SaveCommand`, `DeleteCommand` |
| `Views/` | `XView.xaml` | `UserControl`: DataGrid + form nhập bên dưới, chỉ Binding |

Sau đó đăng ký điều hướng trong `MainWindow.xaml` bằng một `DataTemplate` (`DataType="{x:Type vm:XViewModel}"` → `<views:XView/>`). Sửa `MainWindow.xaml` là sửa file chung: nhắc người dùng báo nhóm trước.

Mẫu đầy đủ của từng file: [references/code-templates.md](references/code-templates.md).

## Quy tắc từng lớp và lý do

**Model.** Chỉ có thuộc tính và `OnPropertyChanged`. Không có SQL, không có logic giao diện.

**Repository.**
- Luôn dùng tham số `@ten` (`cmd.Parameters.AddWithValue`), không bao giờ nối chuỗi vào câu SQL: nối chuỗi gây SQL injection và lỗi dấu nháy với tên tiếng Việt.
- Mở kết nối qua `DbHelper.GetConnection()` trong `using`, để chuỗi kết nối chỉ nằm một chỗ và kết nối luôn được đóng.
- Lỗi SQL để nổi lên dưới dạng `SqlException`; ViewModel bắt và đổi thành câu dễ hiểu.
- Truy vấn cần tên loại, tên NCC… thì dùng `JOIN`, không gọi nhiều lần trong vòng lặp.

**ViewModel.**
- Danh sách là `ObservableCollection<T>`, không phải `List<T>`: `List` không báo cho giao diện khi thêm/xóa nên DataGrid không cập nhật.
- Mỗi setter gọi `OnPropertyChanged()`; thiếu dòng này là nguyên nhân số một của lỗi "giao diện không đổi".
- Nút bấm dùng `RelayCommand`. `CanExecute` của `SaveCommand` trả `false` khi dữ liệu chưa hợp lệ, nên nút Lưu tự mờ đi.
- Kiểm tra nhập liệu theo một cách duy nhất: thuộc tính `ErrorMessage` (chuỗi) bind vào `TextBlock` màu đỏ. Kiểm tra ô trống, số âm, chữ trong ô số, ngày không hợp lệ.
- Mọi lời gọi Repository nằm trong `try/catch`. `SqlException` số 547 (khóa ngoại) → "Không xóa được vì … đang được dùng"; 2627/2601 (trùng khóa) → "… đã tồn tại".
- Không gọi `MessageBox`, không tham chiếu control, không `using System.Windows.Controls`. Lý do: ViewModel phải chạy được mà không cần cửa sổ, và giảng viên hay hỏi đúng điểm này.

**View.**
- Chỉ XAML và Binding. Không dùng sự kiện `Click`; dùng `Command="{Binding SaveCommand}"`. Code-behind chỉ để `InitializeComponent()` và việc thuần giao diện như focus.
- Layout: `Grid` cho form, `StackPanel` cho hàng nút, `DockPanel` cho khung. Kích thước dùng `*` và `Auto`, không đặt `Width`/`Height` cứng cho vùng chứa, để cửa sổ co giãn tốt (tiêu chí Giao diện).
- Style lấy từ `Resources/Styles.xaml`, không đặt màu và font rời rạc trong từng màn.
- DataGrid: `ItemsSource="{Binding Items}"`, `SelectedItem="{Binding SelectedItem}"`, `AutoGenerateColumns="False"`, `IsReadOnly="True"`; sửa dữ liệu qua form bên dưới.

## Trường hợp đặc biệt

- **PasswordBox** (Đăng nhập, Đăng ký): `Password` không bind được. Truyền chính `PasswordBox` qua `CommandParameter="{Binding ElementName=pwd}"`, trong Command đọc `((PasswordBox)p).Password` rồi băm SHA-256 ngay. Đây là ngoại lệ duy nhất được phép đụng tới control, giải thích điều này khi viết.
- **Phiếu nhiều dòng** (Nhập hàng, Bán hàng): ViewModel giữ `ObservableCollection<ChiTiet>` tạm trong bộ nhớ, `TongTien` tính lại mỗi khi danh sách đổi (`CollectionChanged`). Lưu cả phiếu một lần bằng `SqlTransaction` trong Repository, xem skill `pharmacy-database`.
- **Ẩn menu theo vai trò**: thuộc tính `bool LaQuanLy` trên ViewModel của MainWindow + `BooleanToVisibilityConverter`.
- **Tô màu dòng** (Tồn kho, Hạn dùng): `DataGrid.RowStyle` với `DataTrigger` bind vào thuộc tính `bool` như `SapHetHang`, `SapHetHan` tính sẵn trong Model, không dùng converter phức tạp.

## Kết thúc

Sau khi viết xong:
1. Build (`dotnet build` hoặc Ctrl+Shift+B) và chạy thử: thêm, sửa, xóa, nhập sai từng ô.
2. Chạy checklist trong skill `pharmacy-review-checklist`.
3. Tóm tắt cho người dùng 3–5 dòng: Binding nào nối với thuộc tính nào, Command nào làm gì, vì sao dùng `ObservableCollection`. Mục đích là để họ tự trả lời được khi vấn đáp, đừng chỉ đưa code.
