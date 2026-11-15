using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;

namespace PharmacyManagement.Data;

public class ThuocRepository
{
    public List<Thuoc> GetAll()
    {
        var items = new List<Thuoc>();
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            SELECT t.MaThuoc, t.TenThuoc, t.DonViTinh, t.GiaBan, t.SoLuongTon,
                   t.HanSuDung, t.MaLoai, l.TenLoai
            FROM dbo.Thuoc AS t
            INNER JOIN dbo.LoaiThuoc AS l ON l.MaLoai = t.MaLoai
            ORDER BY t.TenThuoc, t.MaThuoc
            """, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new Thuoc
            {
                MaThuoc = reader.GetInt32(0),
                TenThuoc = reader.GetString(1),
                DonViTinh = reader.GetString(2),
                GiaBan = reader.GetDecimal(3),
                SoLuongTon = reader.GetInt32(4),
                HanSuDung = reader.GetDateTime(5),
                MaLoai = reader.GetInt32(6),
                TenLoai = reader.GetString(7)
            });
        }
        return items;
    }

    public int Insert(Thuoc item)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            INSERT INTO dbo.Thuoc (TenThuoc, DonViTinh, GiaBan, SoLuongTon, HanSuDung, MaLoai)
            OUTPUT INSERTED.MaThuoc
            VALUES (@TenThuoc, @DonViTinh, @GiaBan, @SoLuongTon, @HanSuDung, @MaLoai)
            """, conn);
        AddParameters(cmd, item);
        return (int)cmd.ExecuteScalar()!;
    }

    public bool Update(Thuoc item, int soLuongTonBanDau)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("""
            UPDATE dbo.Thuoc
            SET TenThuoc = @TenThuoc, DonViTinh = @DonViTinh, GiaBan = @GiaBan,
                SoLuongTon = @SoLuongTon, HanSuDung = @HanSuDung, MaLoai = @MaLoai
            WHERE MaThuoc = @MaThuoc AND SoLuongTon = @SoLuongTonBanDau
            """, conn);
        AddParameters(cmd, item);
        cmd.Parameters.Add("@MaThuoc", SqlDbType.Int).Value = item.MaThuoc;
        cmd.Parameters.Add("@SoLuongTonBanDau", SqlDbType.Int).Value = soLuongTonBanDau;
        return cmd.ExecuteNonQuery() > 0;
    }

    public void Delete(int maThuoc)
    {
        using var conn = DbHelper.GetConnection();
        using var cmd = new SqlCommand("DELETE FROM dbo.Thuoc WHERE MaThuoc = @MaThuoc", conn);
        cmd.Parameters.Add("@MaThuoc", SqlDbType.Int).Value = maThuoc;
        cmd.ExecuteNonQuery();
    }

    private static void AddParameters(SqlCommand cmd, Thuoc item)
    {
        cmd.Parameters.Add("@TenThuoc", SqlDbType.NVarChar, 200).Value = item.TenThuoc;
        cmd.Parameters.Add("@DonViTinh", SqlDbType.NVarChar, 50).Value = item.DonViTinh;
        var giaBan = cmd.Parameters.Add("@GiaBan", SqlDbType.Decimal);
        giaBan.Precision = 18;
        giaBan.Scale = 0;
        giaBan.Value = item.GiaBan;
        cmd.Parameters.Add("@SoLuongTon", SqlDbType.Int).Value = item.SoLuongTon;
        cmd.Parameters.Add("@HanSuDung", SqlDbType.Date).Value = item.HanSuDung;
        cmd.Parameters.Add("@MaLoai", SqlDbType.Int).Value = item.MaLoai;
    }
}
