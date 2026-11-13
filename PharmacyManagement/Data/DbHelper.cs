using Microsoft.Data.SqlClient;

namespace PharmacyManagement.Data;

public static class DbHelper
{
    // Cả nhóm dùng cùng instance và tên CSDL; chỉ giữ chuỗi kết nối ở đây.
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;TrustServerCertificate=True";

    // Bên gọi sở hữu kết nối đã mở và phải dùng using để đóng kết nối.
    public static SqlConnection GetConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            // Nếu Open lỗi thì bên gọi chưa nhận được kết nối để tự Dispose.
            // Giữ nguyên SqlException để ViewModel xử lý Number và báo lỗi.
            connection.Dispose();
            throw;
        }
    }
}
