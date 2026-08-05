namespace Unify.Erp.Contracts.Products;

public sealed record ProductCategoryResponse(
    Guid Id,
    Guid OrganisationId,
    string Code,
    string Name);
