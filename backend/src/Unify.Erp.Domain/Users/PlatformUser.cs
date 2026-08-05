using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Users;

public sealed class PlatformUser : Entity
{
    public PlatformUser(Guid id, string email, string displayName, UserStatus status = UserStatus.Active)
        : base(id)
    {
        Email = Guard.RequiredText(email, nameof(email), 254).ToLowerInvariant();
        DisplayName = Guard.RequiredText(displayName, nameof(displayName), 120);
        Status = status;
    }

    public string Email { get; }

    public string DisplayName { get; }

    public UserStatus Status { get; private set; }

    public bool CanAuthenticate => Status == UserStatus.Active;

    public void Disable()
    {
        Status = UserStatus.Disabled;
    }
}
