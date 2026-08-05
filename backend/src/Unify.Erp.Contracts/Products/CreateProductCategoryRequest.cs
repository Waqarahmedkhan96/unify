namespace Unify.Erp.Contracts.Products;

public sealed record CreateProductCategoryRequest(
    Guid OrganisationId,
    string Code,
    string Name);
