using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Products;

public sealed record ListProductsRequest(
    Guid OrganisationId,
    Guid? CategoryId,
    string? Search,
    PagedRequest Page);
