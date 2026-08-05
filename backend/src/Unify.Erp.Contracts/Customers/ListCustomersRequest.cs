using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Customers;

public sealed record ListCustomersRequest(
    Guid OrganisationId,
    Guid? BranchId,
    string? Search,
    PagedRequest Page);
