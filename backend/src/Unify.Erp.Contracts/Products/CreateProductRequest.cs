namespace Unify.Erp.Contracts.Products;

public sealed record CreateProductRequest(
    Guid OrganisationId,
    Guid UnitOfMeasureId,
    Guid? CategoryId,
    string ProductCode,
    string Name,
    string? Barcode,
    decimal PurchasePrice,
    decimal SalesPrice,
    bool IsInventoryTracked);
