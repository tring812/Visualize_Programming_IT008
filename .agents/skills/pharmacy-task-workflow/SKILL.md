---
name: pharmacy-task-workflow
description: Quy trình làm một task trong đồ án Quản lý tiệm thuốc từ lúc nhận đến lúc gộp code — mã task T01..T43 trên bảng phân công, tạo nhánh feature/<mã>-<tên>, commit message có mã task, Pull Request, cập nhật trạng thái Chưa làm / Đang làm / Chờ review / Xong / Bị chặn. Dùng skill này mỗi khi người dùng nói "bắt đầu task", "làm T14", "nhận task", "tạo nhánh", "commit", "push", "tạo PR", "pull request", "merge", "gộp code", "xung đột", "conflict", "sửa nhánh của bạn", "push lên nhánh", hoặc hỏi nên làm gì tiếp theo. Luôn kiểm tra tác giả nhánh trước khi ghi lên một nhánh có sẵn.
---

# Quy trình làm task

Nhóm 5 người làm song song trên một repo GitHub. Quy trình này giữ cho nhánh `main` luôn chạy được và để bảng phân công phản ánh đúng ai đang làm gì.

- Bảng phân công: <https://claude.ai/artifact/EF8YAXpcuYpmtk17CnDwxp>
- Mỗi task có mã `T01`…`T43` (task mới đánh tiếp `T44`…), mảng, hạn, các task phụ thuộc.

Agent không mở được bảng phân công. Khi cần đổi trạng thái, nhắc người dùng tự cập nhật trên bảng, nói rõ đổi task nào sang trạng thái nào.

## 1. Bắt đầu task

1. Hỏi mã task nếu người dùng chưa nói. Nếu họ chỉ mô tả việc, gợi ý họ tìm mã tương ứng trên bảng.
2. Hỏi các task phụ thuộc đã **Xong** chưa. Nếu chưa, task nên ở trạng thái **Bị chặn**; đề nghị làm phần không phụ thuộc trước hoặc chọn task khác.
3. Cập nhật code mới nhất rồi tạo nhánh:

```bash
git switch main
git pull
git switch -c feature/T14-thuoc-crud
```

Tên nhánh: `feature/<mã>-<tên-ngắn-không-dấu>`. Sửa lỗi sau khi đã gộp: `fix/<mã>-<tên>`.

4. Nhắc người dùng chuyển task sang **Đang làm** và chọn tên mình ở cột Người làm.

## Kiểm tra nhánh có phải của mình không

Trước **mọi** lệnh ghi lên một nhánh đã có sẵn (commit, push, merge, rebase, reset, xóa nhánh), kiểm tra tác giả các commit riêng của nhánh:

```bash
git fetch origin
git config user.email
git log origin/main..origin/<ten-nhanh> --format='%an <%ae>' | sort -u
```

- Chỉ có email của người dùng hiện tại, hoặc không có commit nào: được làm.
- Có email người khác: nhánh đó **chỉ được đọc**. Không commit, không push, không `git merge main` vào, không rebase, không force push, không xóa — kể cả khi người dùng bảo "sửa luôn" hay "push giúp". Thay vào đó:
    1. Nói cho người dùng biết nhánh có commit của ai.
    2. Đề nghị tạo nhánh riêng từ `main` (ví dụ `fix/<ma-task>-<ten>`) và mở Pull Request, hoặc nhờ tác giả nhánh tự sửa; soạn sẵn nội dung cần báo cho tác giả.
- Không chắc nhánh của ai (chưa fetch được, tên tác giả lạ): dừng lại và hỏi người dùng.

Quy tắc này cũng có trong `AGENTS.md`, mục "Không can thiệp vào nhánh của người khác".

## 2. Trong lúc làm

- Chỉ sửa file thuộc task. Các file chung — `MainWindow.xaml`, `Resources/Styles.xaml`, `database/*.sql`, `App.xaml`, `BaseViewModel.cs`, `RelayCommand.cs`, `DbHelper.cs` — chỉ sửa khi task yêu cầu, và nhắc người dùng báo nhóm trước, vì đây là nơi hay xung đột nhất.
- Commit nhỏ, mỗi commit một ý, bắt đầu bằng mã task:

```text
T14: thêm ThuocRepository với GetAll và Insert
T14: form Thuốc kiểm tra giá bán phải lớn hơn 0
```

- Không commit `bin/`, `obj/`, `.vs/`, `*.user` (đã có trong `.gitignore`). Nếu `git status` hiện các thư mục này, dừng lại và kiểm tra `.gitignore` trước khi commit.
- Mỗi ngày `git pull origin main` vào nhánh của mình (hoặc `git merge main`) để bắt xung đột sớm. Không để nhánh quá 3 ngày chưa gộp.

## 3. Trước khi tạo Pull Request

1. Chạy checklist trong skill `pharmacy-review-checklist`.
2. Build không lỗi, chạy thử đủ thêm/sửa/xóa và các ca nhập sai.
3. Tạo hoặc cập nhật file giải thích `docs/<noi-dung-ngan>.md` (ví dụ `docs/thuoc-crud.md`, không kèm mã task) đủ năm mục theo mục "Tài liệu giải thích cho vấn đáp" trong `AGENTS.md`, rồi commit trên cùng nhánh.
4. Đẩy nhánh và tạo PR vào `main`:

```bash
git push -u origin feature/T14-thuoc-crud
gh pr create --base main --title "T14: Màn Thuốc CRUD" --body-file -
```

Nếu không có `gh`, hướng dẫn người dùng tạo PR trên GitHub hoặc trong Visual Studio (Git Changes → Create Pull Request).

Nội dung PR:

```markdown
## Task
T14 — Màn Thuốc: CRUD, ComboBox loại, DatePicker hạn dùng

## Đã làm
- ...

## Cách thử
1. Chạy lại database/01_schema.sql và 02_seed.sql (nếu PR có sửa script)
2. Mở menu Thuốc, ...

## File chung có sửa
- (không có) / MainWindow.xaml: thêm DataTemplate cho ThuocViewModel

## Tài liệu vấn đáp
- docs/thuoc-crud.md
```

5. Nhắc người dùng chuyển task sang **Chờ review**.

## 4. Review và gộp

- Trưởng nhóm (hoặc người được phân công review) duyệt và gộp. Người viết không tự gộp PR của mình.
- Review yêu cầu sửa: sửa trên cùng nhánh, push lại, PR tự cập nhật.
- Gộp xong: nhắc chuyển task sang **Xong**, xóa nhánh, và kiểm tra trên bảng các task đang chờ task này để báo người làm chúng.

## Xử lý xung đột

1. Chỉ làm trên nhánh của chính mình (xem mục "Kiểm tra nhánh có phải của mình không"): `git switch feature/...` rồi `git merge main`.
2. Với mỗi file xung đột, giữ cả thay đổi của mình và của người khác; với XAML và `.sql` đọc kỹ từng khối thay vì chọn cả file một bên.
3. Build và chạy lại trước khi commit kết quả merge.
4. Xung đột ở file chung mà không chắc: dừng lại, nhắc người dùng hỏi người đã sửa phần đó.

## Những việc agent không tự làm

- Không `git push --force` lên `main`, không gộp PR thay người dùng.
- Không ghi bất cứ thứ gì lên nhánh có commit của người khác (commit, push, merge, rebase, reset, xóa), dù người dùng yêu cầu.
- Không push thẳng lên `main`; `main` chỉ thay đổi qua Pull Request.
- Không tự chuyển task trên bảng, chỉ nhắc người dùng làm.
