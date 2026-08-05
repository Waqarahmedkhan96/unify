namespace Unify.Erp.Contracts.Products;

public sealed record UnitOfMeasureResponse(
    Guid Id,
    Guid OrganisationId,
    string Code,
    string Name,
    int DecimalPlaces);
