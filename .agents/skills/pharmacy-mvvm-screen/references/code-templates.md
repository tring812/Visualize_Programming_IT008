# Mẫu code cho một màn hình CRUD

Ví dụ dùng bảng `LoaiThuoc (MaLoai INT IDENTITY PK, TenLoai NVARCHAR(100))`. Namespace gốc giả định là `PharmacyManagement`; đổi theo project thật. Khi làm màn khác, đổi tên lớp, cột và câu kiểm tra, giữ nguyên cấu trúc.

## Mục lục

1. Nền tảng dùng chung: `BaseViewModel`, `RelayCommand`, `DbHelper`
2. Model
3. Repository
4. ViewModel
5. View (XAML)
6. Đăng ký điều hướng trong MainWindow

---

## 1. Nền tảng dùng chung (chỉ tạo một lần, task T08/T09)

`ViewModels/BaseViewModel.cs`

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PharmacyManagement.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Gán giá trị và báo giao diện khi giá trị thật sự đổi
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
```

`ViewModels/RelayCommand.cs`

```csharp
using System;
using System.Windows.Input;

namespace PharmacyManagement.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);

        // WPF tự hỏi lại CanExecute khi người dùng gõ phím, bấm chuột
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
```

`Data/DbHelper.cs`

```csharp
using Microsoft.Data.SqlClient;

namespace PharmacyManagement.Data
{
    public static class DbHelper
    {
        // Chuỗi kết nối chỉ nằm ở đây, giống nhau trên mọi máy của nhóm
        private const string ConnectionString =
            @"Server=.\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
```

## 2. Model — `Models/LoaiThuoc.cs`

```csharp
using PharmacyManagement.ViewModels;

namespace PharmacyManagement.Models
{
    public class LoaiThuoc : BaseViewModel
    {
        private int _maLoai;
        private string _tenLoai = "";

        public int MaLoai { get => _maLoai; set => SetProperty(ref _maLoai, value); }
        public string TenLoai { get => _tenLoai; set => SetProperty(ref _tenLoai, value); }
    }
}
```

## 3. Repository — `Data/LoaiThuocRepository.cs`

```csharp
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;

namespace PharmacyManagement.Data
{
    public class LoaiThuocRepository
    {
        public List<LoaiThuoc> GetAll()
        {
            var list = new List<LoaiThuoc>();
            using var conn = DbHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT MaLoai, TenLoai FROM LoaiThuoc ORDER BY TenLoai", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new LoaiThuoc
                {
                    MaLoai = reader.GetInt32(0),
                    TenLoai = reader.GetString(1)
                });
            }
            return list;
        }

        public int Insert(LoaiThuoc item)
        {
            using var conn = DbHelper.GetConnection();
            using var cmd = new SqlCommand(
                "INSERT INTO LoaiThuoc (TenLoai) OUTPUT INSERTED.MaLoai VALUES (@TenLoai)", conn);
            cmd.Parameters.AddWithValue("@TenLoai", item.TenLoai);
            return (int)cmd.ExecuteScalar();   // trả về mã vừa tạo
        }

        public void Update(LoaiThuoc item)
        {
            using var conn = DbHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE LoaiThuoc SET TenLoai = @TenLoai WHERE MaLoai = @MaLoai", conn);
            cmd.Parameters.AddWithValue("@TenLoai", item.TenLoai);
            cmd.Parameters.AddWithValue("@MaLoai", item.MaLoai);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int maLoai)
        {
            using var conn = DbHelper.GetConnection();
            using var cmd = new SqlCommand("DELETE FROM LoaiThuoc WHERE MaLoai = @MaLoai", conn);
            cmd.Parameters.AddWithValue("@MaLoai", maLoai);
            cmd.ExecuteNonQuery();
        }
    }
}
```

Cột cho phép NULL: truyền `(object?)giaTri ?? DBNull.Value`. Đọc cột NULL: `reader.IsDBNull(i) ? null : reader.GetString(i)`.

## 4. ViewModel — `ViewModels/LoaiThuocViewModel.cs`

```csharp
using System;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Data;
using PharmacyManagement.Models;

namespace PharmacyManagement.ViewModels
{
    public class LoaiThuocViewModel : BaseViewModel
    {
        private readonly LoaiThuocRepository _repo = new();

        public ObservableCollection<LoaiThuoc> Items { get; } = new();

        private LoaiThuoc? _selectedItem;
        public LoaiThuoc? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                    TenLoai = value.TenLoai;   // đổ dòng đang chọn xuống form
            }
        }

        // Thuộc tính của form nhập
        private string _tenLoai = "";
        public string TenLoai { get => _tenLoai; set => SetProperty(ref _tenLoai, value); }

        private string _errorMessage = "";
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        public RelayCommand AddCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public LoaiThuocViewModel()
        {
            AddCommand = new RelayCommand(_ => ClearForm());
            SaveCommand = new RelayCommand(_ => Save(), _ => IsValid());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);
            Load();
        }

        private void Load()
        {
            try
            {
                Items.Clear();
                foreach (var x in _repo.GetAll()) Items.Add(x);
                ErrorMessage = "";
            }
            catch (SqlException)
            {
                ErrorMessage = "Không kết nối được CSDL. Kiểm tra SQL Server đã chạy chưa.";
            }
        }

        private bool IsValid() => !string.IsNullOrWhiteSpace(TenLoai);

        private void Save()
        {
            var ten = TenLoai.Trim();
            if (ten.Length > 100) { ErrorMessage = "Tên loại tối đa 100 ký tự."; return; }
            try
            {
                if (SelectedItem == null)
                {
                    var item = new LoaiThuoc { TenLoai = ten };
                    item.MaLoai = _repo.Insert(item);
                    Items.Add(item);
                }
                else
                {
                    // Ghi CSDL trước, thành công mới đổi dòng trên DataGrid
                    _repo.Update(new LoaiThuoc { MaLoai = SelectedItem.MaLoai, TenLoai = ten });
                    SelectedItem.TenLoai = ten;
                }
                ClearForm();
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                ErrorMessage = "Tên loại này đã tồn tại.";
            }
            catch (SqlException)
            {
                ErrorMessage = "Lưu không thành công, thử lại.";
            }
        }

        private void Delete()
        {
            if (SelectedItem == null) return;
            try
            {
                _repo.Delete(SelectedItem.MaLoai);
                Items.Remove(SelectedItem);
                ClearForm();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                ErrorMessage = "Không xóa được vì loại này đang có thuốc.";
            }
            catch (SqlException)
            {
                ErrorMessage = "Xóa không thành công, thử lại.";
            }
        }

        private void ClearForm()
        {
            SelectedItem = null;
            TenLoai = "";
            ErrorMessage = "";
        }
    }
}
```

Ô số (giá, số lượng): bind vào thuộc tính `string` trên ViewModel rồi `decimal.TryParse` / `int.TryParse` trong `IsValid()` và báo `ErrorMessage` cụ thể ("Giá bán phải là số lớn hơn 0"). Bind thẳng vào `decimal` khiến chữ gõ sai bị nuốt mất, người dùng không biết vì sao.

Không cần xác nhận xóa bằng `MessageBox` trong ViewModel. Nếu nhóm muốn hộp xác nhận, viết một interface `IDialogService` và truyền vào ViewModel; hỏi nhóm trước khi thêm.

## 5. View — `Views/LoaiThuocView.xaml`

```xml
<UserControl x:Class="PharmacyManagement.Views.LoaiThuocView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>   <!-- tiêu đề -->
            <RowDefinition Height="*"/>      <!-- danh sách -->
            <RowDefinition Height="Auto"/>   <!-- form -->
            <RowDefinition Height="Auto"/>   <!-- lỗi -->
            <RowDefinition Height="Auto"/>   <!-- nút -->
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Text="Loại thuốc" Style="{StaticResource PageTitle}"/>

        <DataGrid Grid.Row="1" ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem}"
                  AutoGenerateColumns="False" IsReadOnly="True" Margin="0,8">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Mã" Binding="{Binding MaLoai}" Width="Auto"/>
                <DataGridTextColumn Header="Tên loại" Binding="{Binding TenLoai}" Width="*"/>
            </DataGrid.Columns>
        </DataGrid>

        <Grid Grid.Row="2">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Label Content="Tên loại" Target="{Binding ElementName=txtTen}"/>
            <TextBox x:Name="txtTen" Grid.Column="1"
                     Text="{Binding TenLoai, UpdateSourceTrigger=PropertyChanged}"/>
        </Grid>

        <TextBlock Grid.Row="3" Text="{Binding ErrorMessage}" Style="{StaticResource ErrorText}"/>

        <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="Thêm mới" Command="{Binding AddCommand}"/>
            <Button Content="Lưu" Command="{Binding SaveCommand}"/>
            <Button Content="Xóa" Command="{Binding DeleteCommand}"/>
        </StackPanel>
    </Grid>
</UserControl>
```

`UpdateSourceTrigger=PropertyChanged` để `CanExecute` của nút Lưu đổi ngay khi gõ, không đợi rời ô. Các style `PageTitle`, `ErrorText` nằm trong `Resources/Styles.xaml`; nếu chưa có thì báo người phụ trách Styles thay vì tự thêm vào file chung.

`LoaiThuocView.xaml.cs` chỉ có constructor gọi `InitializeComponent();`.

## 6. Điều hướng trong MainWindow

```xml
<Window.Resources>
    <DataTemplate DataType="{x:Type vm:LoaiThuocViewModel}">
        <views:LoaiThuocView/>
    </DataTemplate>
</Window.Resources>
...
<ContentControl Content="{Binding CurrentViewModel}"/>
```

Nút menu: `Command="{Binding NavigateCommand}" CommandParameter="LoaiThuoc"`; `MainViewModel` tạo ViewModel tương ứng và gán vào `CurrentViewModel`. WPF tự chọn View nhờ `DataTemplate` theo kiểu dữ liệu.
