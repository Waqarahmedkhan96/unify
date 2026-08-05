using System.Security.Cryptography;
using System.Text;

namespace Unify.Erp.Infrastructure.Auth;

public static class RefreshTokenHasher
{
    public static string Hash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(bytes);
    }
}
