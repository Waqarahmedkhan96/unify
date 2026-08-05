namespace Unify.Erp.Infrastructure.Auth;

public sealed class RefreshTokenRecord
{
    private RefreshTokenRecord()
    {
    }

    public RefreshTokenRecord(
        Guid id,
        Guid userId,
        Guid? organisationId,
        Guid? deviceId,
        Guid familyId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        OrganisationId = organisationId;
        DeviceId = deviceId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? OrganisationId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive(DateTimeOffset nowUtc)
    {
        return RevokedAtUtc is null && ExpiresAtUtc > nowUtc;
    }

    public void Revoke(DateTimeOffset revokedAtUtc, Guid? replacedByTokenId)
    {
        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenId = replacedByTokenId;
    }
}
