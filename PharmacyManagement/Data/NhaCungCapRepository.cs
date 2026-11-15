using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;

namespace PharmacyManagement.Data;

public class NhaCungCapRepository
{
    public List<NhaCungCap> GetAll()
    {
        var items = new List<NhaCungCap>();
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT MaNCC, TenNCC, DiaChi, SoDienThoai FROM dbo.NhaCungCap ORDER BY TenNCC, MaNCC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new NhaCungCap
            {
                MaNCC = reader.GetInt32(0),
                TenNCC = reader.GetString(1),
                DiaChi = reader.IsDBNull(2) ? "" : reader.GetString(2),
                SoDienThoai = reader.GetString(3)
            });
        }
        return items;
    }

    public int Insert(NhaCungCap item)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.NhaCungCap (TenNCC, DiaChi, SoDienThoai) OUTPUT INSERTED.MaNCC VALUES (@TenNCC, @DiaChi, @SoDienThoai)", conn);
        AddParameters(cmd, item);
        return (int)cmd.ExecuteScalar()!;
    }

    public bool Update(NhaCungCap item)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.NhaCungCap SET TenNCC = @TenNCC, DiaChi = @DiaChi, SoDienThoai = @SoDienThoai WHERE MaNCC = @MaNCC", conn);
        AddParameters(cmd, item);
        cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = item.MaNCC;
        return cmd.ExecuteNonQuery() > 0;
    }

    public void Delete(int maNCC)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("DELETE FROM dbo.NhaCungCap WHERE MaNCC = @MaNCC", conn);
        cmd.Parameters.Add("@MaNCC", SqlDbType.Int).Value = maNCC;
        cmd.ExecuteNonQuery();
    }

    private static void AddParameters(SqlCommand cmd, NhaCungCap item)
    {
        cmd.Parameters.Add("@TenNCC", SqlDbType.NVarChar, 200).Value = item.TenNCC;
        cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value =
            string.IsNullOrEmpty(item.DiaChi) ? DBNull.Value : item.DiaChi;
        cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 20).Value = item.SoDienThoai;
    }
}
