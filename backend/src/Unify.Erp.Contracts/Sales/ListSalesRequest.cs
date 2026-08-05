using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Sales;

public sealed record ListSalesRequest(
    Guid OrganisationId,
    Guid? CustomerId,
    PagedRequest Page);
