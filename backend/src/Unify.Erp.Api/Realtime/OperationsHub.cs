using Microsoft.AspNetCore.SignalR;

namespace Unify.Erp.Api.Realtime;

public sealed class OperationsHub : Hub
{
    public Task JoinOrganisation(string organisationId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, OrganisationGroup(organisationId));
    }

    public Task LeaveOrganisation(string organisationId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, OrganisationGroup(organisationId));
    }

    public static string OrganisationGroup(string organisationId)
    {
        return $"organisation:{organisationId}";
    }
}

public sealed record OperationChangedEvent(
    string Module,
    string Action,
    Guid OrganisationId,
    Guid EntityId,
    DateTimeOffset ChangedAtUtc);
