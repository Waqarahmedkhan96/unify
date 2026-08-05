namespace Unify.Erp.Contracts.Products;

public sealed record CreateUnitOfMeasureRequest(
    Guid OrganisationId,
    string Code,
    string Name,
    int DecimalPlaces);
