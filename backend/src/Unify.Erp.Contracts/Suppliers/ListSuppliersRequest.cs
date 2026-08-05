using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Suppliers;

public sealed record ListSuppliersRequest(
    Guid OrganisationId,
    string? Search,
    PagedRequest Page);
