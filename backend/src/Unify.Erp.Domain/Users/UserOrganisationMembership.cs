using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Users;

public sealed class UserOrganisationMembership : TenantEntity
{
    public UserOrganisationMembership(
        Guid id,
        Guid organisationId,
        Guid userId,
        MembershipStatus status = MembershipStatus.Active)
        : base(id, organisationId)
    {
        UserId = Guard.RequiredId(userId, nameof(userId));
        Status = status;
    }

    public Guid UserId { get; }

    public MembershipStatus Status { get; private set; }

    public bool IsActive => Status == MembershipStatus.Active;

    public void Suspend()
    {
        Status = MembershipStatus.Suspended;
    }
}
