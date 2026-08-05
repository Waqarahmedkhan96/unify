using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Inventory;
namespace Unify.Erp.Api.Inventory;

public static class InventoryRequestValidators
{
    public static ValidationResult Validate(this CreateStockAdjustmentRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequiredId(result, nameof(request.WarehouseId), request.WarehouseId);
        AddRequiredId(result, nameof(request.ProductId), request.ProductId);
        if (!string.Equals(request.MovementType, "AdjustmentIn", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.MovementType, "AdjustmentOut", StringComparison.OrdinalIgnoreCase))
        {
            result.Add(nameof(request.MovementType), "Movement type must be AdjustmentIn or AdjustmentOut.");
        }

        AddPositiveQuantity(result, nameof(request.Quantity), request.Quantity);
        return result;
    }

    public static ValidationResult Validate(this CreateStockTransferRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequiredId(result, nameof(request.SourceWarehouseId), request.SourceWarehouseId);
        AddRequiredId(result, nameof(request.DestinationWarehouseId), request.DestinationWarehouseId);
        AddRequiredId(result, nameof(request.ProductId), request.ProductId);
        if (request.SourceWarehouseId == request.DestinationWarehouseId)
        {
            result.Add(nameof(request.DestinationWarehouseId), "Destination warehouse must be different.");
        }

        AddPositiveQuantity(result, nameof(request.Quantity), request.Quantity);
        return result;
    }

    private static void AddRequiredId(ValidationResult result, string field, Guid value)
    {
        if (value == Guid.Empty)
        {
            result.Add(field, "Value is required.");
        }
    }

    private static void AddPositiveQuantity(ValidationResult result, string field, decimal value)
    {
        if (value <= 0)
        {
            result.Add(field, "Value must be greater than zero.");
        }
    }
}
