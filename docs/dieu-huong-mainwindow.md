# Điều hướng MainWindow (T10)

## 1. Mục tiêu

Xây dựng khung cửa sổ chính có menu bên trái và vùng nội dung bên phải bằng `DockPanel`, `ContentControl` và `DataTemplate`. Khi người dùng chọn menu, `NavigateCommand` cập nhật `CurrentViewModel` để WPF hiển thị View tương ứng theo kiến trúc MVVM của nhóm.

## 2. File đã làm

| File | Thành phần | Vai trò |
| --- | --- | --- |
| `PharmacyManagement/ViewModels/MainViewModel.cs` | ViewModel | Giữ màn hình hiện tại, khởi tạo màn mặc định và xử lý lệnh điều hướng |
| `PharmacyManagement/MainWindow.xaml` | View, file chung | Đặt menu ở bên trái, bind nội dung vào `CurrentViewModel` và dùng DataTemplate Loại thuốc đã có |
| `PharmacyManagement/MainWindow.xaml.cs` | Khởi tạo View | Gọi `InitializeComponent()` và gán `MainViewModel` làm `DataContext` của cửa sổ |
| `docs/dieu-huong-mainwindow.md` | Tài liệu giải thích | Giải thích các thành phần điều hướng, hành vi khi chọn menu và câu hỏi vấn đáp |

T10 sử dụng `BaseViewModel` và `RelayCommand` từ phần nền tảng T08. Màn hình thực tế đang được tích hợp là cặp `LoaiThuocViewModel` và `LoaiThuocView` của nhóm.

## 3. Khái niệm

### `MainViewModel` và màn hình mặc định

`MainViewModel` kế thừa `BaseViewModel` trong namespace `PharmacyManagement.ViewModels`. Lớp này giữ trạng thái điều hướng của cửa sổ chính. Các thao tác xem, thêm, sửa, xóa loại thuốc tiếp tục do `LoaiThuocViewModel` đảm nhiệm.

Constructor của `MainViewModel` tạo màn hình mặc định và lệnh menu

```csharp
public MainViewModel()
{
    _currentViewModel = new LoaiThuocViewModel();
    NavigateCommand = new RelayCommand(Navigate, parameter => parameter is "LoaiThuoc");
}
```

Màn hình mặc định là Loại thuốc. Constructor của `LoaiThuocViewModel` gọi `Load()` để đọc danh sách qua `LoaiThuocRepository`. Khi cửa sổ nhận `DataContext`, Binding đọc được `CurrentViewModel` đã khởi tạo và hiển thị màn này.

### `DataContext` là nguồn dữ liệu cho Binding

Trong constructor của `MainWindow`, sau `InitializeComponent()`, có dòng

```csharp
DataContext = new MainViewModel();
```

`DataContext` cung cấp đối tượng nguồn cho các Binding ở cửa sổ và các phần tử con được kế thừa nguồn này. Vì vậy, `{Binding NavigateCommand}` tìm thuộc tính lệnh trên `MainViewModel`, còn `{Binding CurrentViewModel}` đọc ViewModel của màn hình cần hiển thị.

Code-behind đảm nhiệm việc khởi tạo cửa sổ và gán nguồn dữ liệu. Quyết định màn hình cần mở nằm trong phương thức `Navigate` của `MainViewModel`.

### `DockPanel` và menu bên trái

Phần layout chính trong `MainWindow.xaml` là

```xml
<DockPanel>
    <StackPanel DockPanel.Dock="Left" Margin="12">
        <Button Content="Loại thuốc" Padding="16,8"
                Command="{Binding NavigateCommand}" CommandParameter="LoaiThuoc"/>
    </StackPanel>

    <ContentControl Content="{Binding CurrentViewModel}"/>
</DockPanel>
```

`DockPanel` sắp xếp các phần tử con theo cạnh cửa sổ. `DockPanel.Dock="Left"` đưa `StackPanel` menu về phía trái. `StackPanel` xếp các nút theo chiều dọc mặc định và lấy kích thước theo nội dung cùng khoảng cách đã khai báo.

`DockPanel.LastChildFill` mặc định là `true`, nên phần tử cuối cùng là `ContentControl` chiếm vùng còn lại. Khi thay đổi kích thước cửa sổ, vùng nội dung được phân bố lại theo layout này.

Ở thời điểm triển khai T10, project có các style cục bộ của màn Loại thuốc. Menu dùng cách hiển thị mặc định của Button WPF, phù hợp với việc các tài nguyên `Resources/Styles.xaml` và `Resources/Colors.xaml` đang chờ phần giao diện chung của nhóm.

### `CurrentViewModel` và `PropertyChanged`

Thuộc tính đại diện cho màn hình hiện tại được khai báo như sau

```csharp
private BaseViewModel _currentViewModel;

public BaseViewModel CurrentViewModel
{
    get => _currentViewModel;
    private set => SetProperty(ref _currentViewModel, value);
}
```

Kiểu `BaseViewModel` cho phép thuộc tính giữ các ViewModel kế thừa lớp nền của nhóm. Giá trị hiện tại trong T10 là một đối tượng `LoaiThuocViewModel`. Setter đặt `private` để việc đổi màn hình được thực hiện bên trong `MainViewModel`.

`SetProperty` so sánh giá trị cũ với giá trị mới. Khi có thay đổi, hàm gán lại trường dữ liệu rồi phát sự kiện `PropertyChanged` với tên `CurrentViewModel`. Binding trên `ContentControl.Content` nhận sự kiện này và đọc lại nội dung.

Lần khởi tạo đầu tiên gán trực tiếp `_currentViewModel` trong constructor. Binding lấy giá trị ban đầu khi `DataContext` được thiết lập. Những lần điều hướng sau đi qua setter của `CurrentViewModel` để thông báo thay đổi.

### `NavigateCommand`, `CommandParameter` và `CanExecute`

Nút menu nối với lệnh bằng hai thuộc tính

```xml
Command="{Binding NavigateCommand}" CommandParameter="LoaiThuoc"
```

`Command` xác định công việc cần gọi. `CommandParameter` mang định danh màn hình, ở đây là chuỗi chính xác `"LoaiThuoc"`. Nội dung hiển thị của nút là tiếng Việt có dấu, còn định danh truyền vào code dùng tên thống nhất với lớp của dự án.

`RelayCommand` hiện có nhận một `Action<object?>` để thực thi và một `Predicate<object?>` tùy chọn để xét điều kiện thực thi. Trong T10, `Navigate` là hàm thực thi, còn `parameter => parameter is "LoaiThuoc"` là điều kiện `CanExecute`.

WPF hỏi `CanExecute` để biết nút có được phép thực thi hay không. Chuỗi `"LoaiThuoc"` được chấp nhận. `null`, chuỗi khác, số hoặc đối tượng khác nhận kết quả `false`.

Phần thực thi dùng ánh xạ đơn giản trong `switch`

```csharp
private void Navigate(object? parameter)
{
    switch (parameter)
    {
        case "LoaiThuoc":
            CurrentViewModel = new LoaiThuocViewModel();
            break;
    }
}
```

`RelayCommand.Execute` gọi trực tiếp delegate thực thi. Vì vậy, ngay cả khi code C# gọi `Execute` với parameter không hợp lệ, `switch` vẫn xử lý an toàn bằng cách bỏ qua giá trị đó. `CurrentViewModel` tiếp tục giữ màn hình đang hiển thị.

Mỗi lần chọn mục Loại thuốc hợp lệ sẽ tạo một `LoaiThuocViewModel` mới. Danh sách được nạp lại và form trở về trạng thái ban đầu. Nội dung đang gõ dở trong form cũ sẽ được thay bằng trạng thái của màn hình mới.

### `ContentControl` và `DataTemplate` theo kiểu dữ liệu

`ContentControl` có thể hiển thị một nội dung tại một thời điểm. Trong T10, nội dung của nó lấy từ ViewModel hiện tại

```xml
<ContentControl Content="{Binding CurrentViewModel}"/>
```

DataTemplate đã có trong `Window.Resources` mô tả cách hiển thị đối tượng `LoaiThuocViewModel`

```xml
<DataTemplate DataType="{x:Type vm:LoaiThuocViewModel}">
    <views:LoaiThuocView/>
</DataTemplate>
```

`vm` trỏ đến namespace `PharmacyManagement.ViewModels`. `views` trỏ đến `PharmacyManagement.Views`. `DataType` chọn kiểu đối tượng nguồn mà template này áp dụng.

Khi `ContentControl` nhận một `LoaiThuocViewModel`, WPF tìm DataTemplate tương ứng và tạo `LoaiThuocView`. Đối tượng ViewModel ở `Content` trở thành nguồn dữ liệu cho các Binding bên trong View được tạo bởi template. Vì thế, `Items`, `SelectedItem`, `TenLoai` và các lệnh CRUD của View đều được lấy từ `LoaiThuocViewModel` đang hiển thị.

T10 tiếp tục dùng template theo kiểu này khi bổ sung các màn hình đã được nhóm triển khai. Mỗi cặp ViewModel/View cần một ánh xạ phù hợp và một nhánh điều hướng tương ứng.

## 4. Luồng hoạt động

### Khởi động ứng dụng

1. `App.xaml` có `StartupUri="MainWindow.xaml"`, nên WPF tạo cửa sổ chính.
2. Constructor của `MainWindow` gọi `InitializeComponent()` để nạp XAML, menu, vùng nội dung và DataTemplate.
3. `MainWindow` tạo `MainViewModel`. Constructor của ViewModel tạo `LoaiThuocViewModel` làm màn mặc định và thiết lập `NavigateCommand`.
4. `LoaiThuocViewModel` nạp danh sách qua Repository. Khi phát sinh `SqlException`, lớp này đặt thông báo vào `ErrorMessage` theo cách xử lý sẵn có của màn Loại thuốc.
5. `MainWindow` gán đối tượng `MainViewModel` vào `DataContext`.
6. `ContentControl` đọc `CurrentViewModel`, WPF chọn DataTemplate Loại thuốc và tạo View tương ứng.
7. DataGrid trong `LoaiThuocView` đọc `Items` qua Binding để hiển thị danh sách.

### Chọn menu Loại thuốc

```text
Button Loại thuốc
→ NavigateCommand nhận CommandParameter = "LoaiThuoc"
→ CanExecute cho phép thực thi
→ Navigate tạo LoaiThuocViewModel mới
→ CurrentViewModel được gán qua SetProperty
→ PropertyChanged báo CurrentViewModel đã đổi
→ ContentControl cập nhật Content
→ DataTemplate chọn LoaiThuocView
→ View hiển thị danh sách và form của ViewModel mới
```

Menu làm việc với `MainViewModel`, còn các nút Thêm mới, Lưu, Xóa bên trong màn hình làm việc với `LoaiThuocViewModel` do DataTemplate cung cấp. Mỗi ViewModel đảm nhiệm phần trạng thái và lệnh của mình.

### Nhận parameter không hợp lệ

Ví dụ một bên gọi `NavigateCommand.CanExecute(null)` hoặc truyền chuỗi `"Thuoc"` khi màn Thuốc chưa được tích hợp. Điều kiện lệnh trả về `false`.

Nếu bên gọi vẫn gọi trực tiếp `Execute(null)` hoặc `Execute("Thuoc")`, `switch` đi qua mà không khớp nhánh nào. Màn hình đang hiển thị tiếp tục được giữ và lệnh kết thúc bình thường.

### Phạm vi các mục menu

Repository hiện có một cặp ViewModel/View nghiệp vụ cho Loại thuốc, nên menu T10 có một mục tương ứng. Việc kiểm tra chọn qua lại giữa hai chức năng sẽ thực hiện khi các task màn hình khác cung cấp thêm cặp ViewModel/View thực tế.

## 5. Câu hỏi vấn đáp có thể gặp

**DockPanel dùng để làm gì trong T10?**

DockPanel chia khung chính theo các cạnh. Menu được dock sang trái, ContentControl là phần tử cuối cùng và chiếm diện tích còn lại.

**Vì sao dùng StackPanel cho menu?**

StackPanel xếp các nút nối tiếp theo chiều dọc mặc định. Cách này phù hợp với menu bên trái và lấy kích thước theo nội dung.

**ContentControl chứa nội dung gì?**

Thuộc tính `Content` bind vào `CurrentViewModel`, nên nó nhận đối tượng ViewModel của màn hình hiện tại. WPF sử dụng DataTemplate để tạo giao diện cho đối tượng đó.

**CurrentViewModel có nhiệm vụ gì?**

Nó biểu thị màn hình đang được chọn. Khi thuộc tính này thay đổi và phát `PropertyChanged`, vùng nội dung đọc lại giá trị để cập nhật View tương ứng.

**Vì sao MainViewModel kế thừa BaseViewModel?**

Để sử dụng lại `INotifyPropertyChanged` và `SetProperty` của nhóm. Setter của `CurrentViewModel` dùng đúng cơ chế này để báo thay đổi cho Binding.

**Vì sao kiểu của CurrentViewModel là BaseViewModel?**

Các ViewModel màn hình của nhóm cùng kế thừa lớp nền này. Thuộc tính có thể giữ một ViewModel màn hình cụ thể, còn DataTemplate lựa chọn cách hiển thị theo kiểu thực tế của đối tượng.

**DataContext của MainWindow được gán ở đâu?**

Trong constructor của `MainWindow`, sau `InitializeComponent()`, bằng `DataContext = new MainViewModel();`. Các Binding của menu và vùng nội dung lấy nguồn từ đối tượng này.

**DataContext của LoaiThuocView đến từ đâu?**

`ContentControl.Content` nhận đối tượng `LoaiThuocViewModel`. DataTemplate tạo View để hiển thị đối tượng đó, nên các Binding bên trong View nhận nguồn dữ liệu là ViewModel màn hình.

**DataTemplate biết chọn View nào bằng cách nào?**

Thuộc tính `DataType="{x:Type vm:LoaiThuocViewModel}"` gắn template với kiểu ViewModel thực tế. Nội dung template là `<views:LoaiThuocView/>`, nên WPF tạo View này khi hiển thị đối tượng phù hợp.

**NavigateCommand và CommandParameter phối hợp ra sao?**

Nút bind vào `NavigateCommand` và truyền chuỗi `"LoaiThuoc"` làm parameter. Command đưa giá trị này đến `Navigate`, hàm chọn nhánh phù hợp và gán ViewModel mới cho `CurrentViewModel`.

**CanExecute của menu kiểm tra gì?**

Nó kiểm tra parameter có đúng chuỗi `"LoaiThuoc"` hay không. Chỉ định danh đã được tích hợp này được phép thực thi.

**Vì sao trong Navigate vẫn phải xử lý parameter không hợp lệ?**

`RelayCommand.Execute` gọi trực tiếp hàm thực thi. Một lời gọi từ C# có thể đi thẳng đến `Execute`, nên `switch` trong `Navigate` cũng cần kết thúc an toàn với `null`, sai kiểu hoặc định danh chưa hỗ trợ.

**Bấm lại mục Loại thuốc thì chuyện gì xảy ra?**

MainViewModel tạo đối tượng Loại thuốc mới, constructor của đối tượng này nạp lại danh sách. Form mới bắt đầu ở trạng thái trống và PropertyChanged báo vùng nội dung cập nhật theo đối tượng mới.

**Vì sao cách này đúng MVVM?**

View mô tả layout và Binding, MainViewModel giữ trạng thái cùng quyết định điều hướng, còn các ViewModel màn hình giữ nghiệp vụ của mình. Code-behind của cửa sổ chỉ đảm nhiệm khởi tạo giao diện và DataContext.

**Vì sao nút menu dùng Command?**

Command nối thao tác của nút với logic trong ViewModel và cung cấp `CanExecute`. WPF tự gọi lệnh từ Binding khi người dùng bấm nút, nên luồng điều hướng được quản lý trong MainViewModel.

**T10 đã hỗ trợ những màn hình nào?**

Màn Loại thuốc đang có View và ViewModel thực tế. Các màn khác được tích hợp khi thành viên phụ trách hoàn thành cặp ViewModel/View tương ứng, sau đó bổ sung menu, nhánh điều hướng và DataTemplate.
