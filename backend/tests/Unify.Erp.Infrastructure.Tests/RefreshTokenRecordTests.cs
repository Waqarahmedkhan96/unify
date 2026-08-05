using Unify.Erp.Infrastructure.Auth;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class RefreshTokenRecordTests
{
    [Fact]
    public void Active_token_becomes_inactive_after_revoke()
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var token = new RefreshTokenRecord(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            Guid.NewGuid(),
            RefreshTokenHasher.Hash("refresh-token"),
            nowUtc,
            nowUtc.AddDays(1));

        Assert.True(token.IsActive(nowUtc));

        token.Revoke(nowUtc, null);

        Assert.False(token.IsActive(nowUtc));
        Assert.Equal(nowUtc, token.RevokedAtUtc);
    }
}
