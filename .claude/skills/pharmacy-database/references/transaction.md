# Lưu hóa đơn bằng SqlTransaction

Mẫu cho `Data/HoaDonRepository.cs`. Phiếu nhập làm y hệt, chỉ đổi bảng và đổi phép trừ tồn thành phép cộng (không cần điều kiện `SoLuongTon >= @SoLuong`).

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;

namespace PharmacyManagement.Data
{
    public class HoaDonRepository
    {
        /// <summary>
        /// Lưu hóa đơn, các dòng chi tiết và trừ tồn kho trong một transaction.
        /// Trả về mã hóa đơn. Ném InvalidOperationException nếu một thuốc không đủ hàng hoặc đã hết hạn.
        /// </summary>
        public int Save(int maNV, int? maKH, IReadOnlyList<ChiTietHoaDon> chiTiet)
        {
            using var conn = DbHelper.GetConnection();
            using var tran = conn.BeginTransaction();
            try
            {
                decimal tongTien = 0;
                foreach (var ct in chiTiet) tongTien += ct.SoLuong * ct.DonGia;

                // 1. Phần đầu hóa đơn
                int maHD;
                using (var cmd = new SqlCommand(
                    @"INSERT INTO HoaDon (NgayBan, TongTien, MaNV, MaKH)
                      OUTPUT INSERTED.MaHD
                      VALUES (GETDATE(), @TongTien, @MaNV, @MaKH)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@TongTien", tongTien);
                    cmd.Parameters.AddWithValue("@MaNV", maNV);
                    cmd.Parameters.AddWithValue("@MaKH", (object?)maKH ?? DBNull.Value); // khách lẻ = NULL
                    maHD = (int)cmd.ExecuteScalar();
                }

                foreach (var ct in chiTiet)
                {
                    // 2. Trừ tồn: chỉ thành công khi đủ hàng và chưa hết hạn
                    using (var cmd = new SqlCommand(
                        @"UPDATE Thuoc SET SoLuongTon = SoLuongTon - @SoLuong
                          WHERE MaThuoc = @MaThuoc AND SoLuongTon >= @SoLuong
                            AND HanSuDung >= CAST(GETDATE() AS DATE)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                        cmd.Parameters.AddWithValue("@MaThuoc", ct.MaThuoc);
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException($"{ct.TenThuoc} không đủ hàng hoặc đã hết hạn.");
                    }

                    // 3. Dòng chi tiết, lưu giá tại thời điểm bán
                    using (var cmd = new SqlCommand(
                        @"INSERT INTO CT_HoaDon (MaHD, MaThuoc, SoLuong, DonGia)
                          VALUES (@MaHD, @MaThuoc, @SoLuong, @DonGia)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@MaHD", maHD);
                        cmd.Parameters.AddWithValue("@MaThuoc", ct.MaThuoc);
                        cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                        cmd.Parameters.AddWithValue("@DonGia", ct.DonGia);
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
                return maHD;
            }
            catch
            {
                tran.Rollback();   // một bước lỗi thì hủy toàn bộ
                throw;
            }
        }
    }
}
```

Điểm cần giải thích được khi vấn đáp:

- Mọi `SqlCommand` trong transaction phải nhận `tran` ở tham số thứ ba, nếu không sẽ lỗi "ExecuteNonQuery requires the command to have a transaction".
- `throw;` sau `Rollback()` giữ nguyên lỗi gốc để ViewModel bắt và hiện `ErrorMessage`.
- ViewModel bắt `InvalidOperationException` (hiện đúng câu trong lỗi) và `SqlException` (hiện câu chung "Thanh toán không thành công").
- Giỏ hàng gộp các dòng cùng `MaThuoc` trước khi lưu, vì PK của `CT_HoaDon` là (`MaHD`, `MaThuoc`).
