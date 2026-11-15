using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;

namespace PharmacyManagement.Data;

public class KhachHangRepository
{
    public List<KhachHang> GetAll() => SearchBySdt("");

    public List<KhachHang> SearchBySdt(string sdt)
    {
        var items = new List<KhachHang>();
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            SELECT MaKH, HoTen, SoDienThoai, DiaChi
            FROM dbo.KhachHang
            WHERE SoDienThoai LIKE @Sdt + '%'
            ORDER BY HoTen, MaKH
            """, conn);
        cmd.Parameters.Add("@Sdt", SqlDbType.VarChar, 20).Value = sdt;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            items.Add(ReadCustomer(reader));
        return items;
    }

    public KhachHang? GetBySdt(string sdt)
    {
        sdt = sdt.Trim();
        if (sdt.Length is < 1 or > 20)
            return null;

        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            SELECT MaKH, HoTen, SoDienThoai, DiaChi
            FROM dbo.KhachHang
            WHERE SoDienThoai = @Sdt
            """, conn);
        cmd.Parameters.Add("@Sdt", SqlDbType.VarChar, 20).Value = sdt;
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? ReadCustomer(reader) : null;
    }

    public int Insert(KhachHang item)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            INSERT INTO dbo.KhachHang (HoTen, SoDienThoai, DiaChi)
            OUTPUT INSERTED.MaKH
            VALUES (@HoTen, @SoDienThoai, @DiaChi)
            """, conn);
        AddParameters(cmd, item);
        return (int)cmd.ExecuteScalar()!;
    }

    public bool Update(KhachHang item)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            UPDATE dbo.KhachHang
            SET HoTen = @HoTen, SoDienThoai = @SoDienThoai, DiaChi = @DiaChi
            WHERE MaKH = @MaKH
            """, conn);
        AddParameters(cmd, item);
        cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = item.MaKH;
        return cmd.ExecuteNonQuery() > 0;
    }

    public void Delete(int maKH)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("DELETE FROM dbo.KhachHang WHERE MaKH = @MaKH", conn);
        cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKH;
        cmd.ExecuteNonQuery();
    }

    private static KhachHang ReadCustomer(SqlDataReader reader) => new()
    {
        MaKH = reader.GetInt32(0),
        HoTen = reader.GetString(1),
        SoDienThoai = reader.GetString(2),
        DiaChi = reader.IsDBNull(3) ? "" : reader.GetString(3)
    };

    private static void AddParameters(SqlCommand cmd, KhachHang item)
    {
        cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = item.HoTen;
        cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 20).Value = item.SoDienThoai;
        cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value =
            string.IsNullOrEmpty(item.DiaChi) ? DBNull.Value : item.DiaChi;
    }
}
