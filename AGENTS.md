# Hướng dẫn cho agent AI

Repo này là đồ án nhóm 5 sinh viên môn IT008 (Lập trình trực quan, UIT): ứng dụng desktop **WPF (.NET 8) quản lý tiệm thuốc**, CSDL **SQL Server** truy xuất bằng **ADO.NET** (`Microsoft.Data.SqlClient`), kiến trúc **MVVM tự viết**. Đọc `README.md` để nắm phạm vi chức năng, cấu trúc thư mục và quy ước.

## Skill của dự án

Các skill nằm ở `.agents/skills/` (bản sao cho Claude Code ở `.claude/skills/`). Dùng skill phù hợp trước khi làm:

| Skill | Khi nào |
| --- | --- |
| `pharmacy-mvvm-screen` | Làm hoặc sửa bất kỳ màn hình, ViewModel, View, Command, Binding nào |
| `pharmacy-database` | Sửa script `database/*.sql`, viết SQL, Repository, transaction, xử lý lỗi SQL |
| `pharmacy-task-workflow` | Bắt đầu task, tạo nhánh, commit, Pull Request, xử lý xung đột |
| `pharmacy-review-checklist` | Tự kiểm tra trước khi tạo PR hoặc review PR của bạn cùng nhóm |

## Nguyên tắc bắt buộc

- **MVVM là bắt buộc** (3/10 điểm): View chỉ có XAML và Binding; nút dùng `Command`, không dùng sự kiện `Click`; ViewModel không gọi `MessageBox` hay đụng control; danh sách là `ObservableCollection<T>`.
- **SQL chỉ nằm trong `Data/*Repository.cs`**, luôn dùng tham số `@ten`, không nối chuỗi. Chuỗi kết nối chỉ ở `Data/DbHelper.cs`.
- **Không thêm thư viện** (MVVM toolkit, Entity Framework, Dapper…) khi nhóm chưa đồng ý: đồ án chấm phần tự viết `INotifyPropertyChanged`, `ICommand` và ADO.NET.
- **Không thêm chức năng ngoài phạm vi** trong README (biểu đồ, quản lý lô, in hóa đơn…). Gợi ý ghi vào mục "Hướng phát triển" của báo cáo.
- **File chung** (`MainWindow.xaml`, `Resources/Styles.xaml`, `database/*.sql`, `App.xaml`, `BaseViewModel.cs`, `RelayCommand.cs`, `DbHelper.cs`): chỉ sửa khi task yêu cầu, và nhắc người dùng báo nhóm.
- **Mỗi task phải có tài liệu giải thích trong `docs/`** (xem mục dưới). Sau khi viết code, vẫn tóm tắt 3–5 dòng trong chat cách Binding và Command hoạt động trong phần vừa viết, đừng chỉ đưa code.

## Tài liệu giải thích cho vấn đáp

Mỗi người phải tự giải thích được code của mình khi vấn đáp. Vì vậy khi làm bất kỳ task nào, luôn tạo (hoặc cập nhật nếu đã có) **một** file giải thích trong thư mục `docs/`:

- **Tên file**: nội dung công việc viết ngắn gọn, chữ thường không dấu nối bằng `-`, **không kèm mã task**. Ví dụ: `docs/thuoc-crud.md`, `docs/dang-nhap.md`, `docs/nen-tang-mvvm-ket-noi-sql.md`.
- **Nội dung** viết tiếng Việt có dấu, dễ hiểu với sinh viên, gồm đủ năm mục:
  1. **Mục tiêu**: task làm gì, 1–2 câu.
  2. **File đã làm**: mỗi file một dòng, nói rõ vai trò (Model, Repository, ViewModel, View).
  3. **Khái niệm**: giải thích từng khái niệm dùng trong task, gắn với code thật của task (Binding, Command, `CanExecute`, `ObservableCollection`, tham số `@ten`, transaction…).
  4. **Luồng hoạt động**: đi theo một thao tác cụ thể, ví dụ người dùng bấm nút → Command → ViewModel → Repository → SQL → giao diện cập nhật.
  5. **Câu hỏi vấn đáp có thể gặp**: mỗi câu kèm câu trả lời ngắn.
- Không ghi nhật ký môi trường hay kết quả build vào file này; những thứ đó để trong mô tả PR.
- Commit file này trên cùng nhánh của task, trước khi tạo Pull Request.

## Lệnh hay dùng

```bash
dotnet build
dotnet run --project PharmacyManagement
git switch -c feature/T14-thuoc-crud
```

CSDL: chạy `database/01_schema.sql` rồi `database/02_seed.sql` trong SSMS, instance `.\SQLEXPRESS`.

## Ngôn ngữ

Trả lời người dùng bằng tiếng Việt. Tên bảng, cột, lớp, biến theo quy ước tiếng Việt không dấu PascalCase như trong README.
