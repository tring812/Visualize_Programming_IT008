using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;

namespace PharmacyManagement.Data;

// Nơi duy nhất chứa câu SQL của bảng LoaiThuoc.
// Mọi giá trị đưa vào SQL đều qua tham số @ten, không nối chuỗi.
// Lỗi SQL để nổi lên dạng SqlException; ViewModel bắt và đổi thành câu dễ hiểu.
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

    // Trả về MaLoai do SQL Server tự sinh (cột IDENTITY).
    public int Insert(LoaiThuoc item)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO LoaiThuoc (TenLoai) OUTPUT INSERTED.MaLoai VALUES (@TenLoai)", conn);
        cmd.Parameters.AddWithValue("@TenLoai", item.TenLoai);
        return (int)cmd.ExecuteScalar()!;
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

    // Loại đang có thuốc sẽ bị khóa ngoại FK_Thuoc_LoaiThuoc chặn (SqlException số 547).
    public void Delete(int maLoai)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("DELETE FROM LoaiThuoc WHERE MaLoai = @MaLoai", conn);
        cmd.Parameters.AddWithValue("@MaLoai", maLoai);
        cmd.ExecuteNonQuery();
    }
}
