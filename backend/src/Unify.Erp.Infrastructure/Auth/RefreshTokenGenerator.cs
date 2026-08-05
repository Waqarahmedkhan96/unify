using System.Security.Cryptography;

namespace Unify.Erp.Infrastructure.Auth;

public static class RefreshTokenGenerator
{
    private const int TokenByteLength = 64;

    public static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);

        return Convert.ToBase64String(bytes);
    }
}
