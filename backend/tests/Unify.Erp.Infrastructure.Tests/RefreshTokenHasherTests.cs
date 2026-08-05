using Unify.Erp.Infrastructure.Auth;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class RefreshTokenHasherTests
{
    [Fact]
    public void Hash_is_stable_and_does_not_return_the_raw_token()
    {
        const string token = "sample-refresh-token";

        var first = RefreshTokenHasher.Hash(token);
        var second = RefreshTokenHasher.Hash(token);

        Assert.Equal(first, second);
        Assert.NotEqual(token, first);
        Assert.Equal(64, first.Length);
    }
}
