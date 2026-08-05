using Unify.Erp.Contracts.Audit;
using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Application.Audit;

public interface IAuditService
{
    Task<PagedResponse<AuditEntryResponse>> ListAsync(
        ListAuditEntriesRequest request,
        CancellationToken cancellationToken);
}
