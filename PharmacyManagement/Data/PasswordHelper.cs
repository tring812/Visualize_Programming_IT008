using System.Security.Cryptography;
using System.Text;

namespace PharmacyManagement.Data;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
