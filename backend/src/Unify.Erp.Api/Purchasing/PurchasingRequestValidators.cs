using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Purchasing;

namespace Unify.Erp.Api.Purchasing;

public static class PurchasingRequestValidators
{
    public static ValidationResult Validate(this CreatePurchaseOrderRequest request)
    {
        var result = ValidateHeader(request.OrganisationId, request.BranchId, request.SupplierId, request.OrderNumber, request.Items.Count);
        ValidateLines(result, request.Items);
        return result;
    }

    public static ValidationResult Validate(this CreateSupplierInvoiceRequest request)
    {
        var result = ValidateHeader(request.OrganisationId, request.BranchId, request.SupplierId, request.InvoiceNumber, request.Items.Count);
        ValidateLines(result, request.Items);
        return result;
    }

    public static ValidationResult Validate(this CreateGoodsReceiptRequest request)
    {
        var result = ValidateHeader(request.OrganisationId, request.BranchId, request.SupplierId, request.ReceiptNumber, request.Items.Count);
        AddRequiredId(result, nameof(request.WarehouseId), request.WarehouseId);
        foreach (var item in request.Items)
        {
            AddRequiredId(result, nameof(item.ProductId), item.ProductId);
            AddRequired(result, nameof(item.Description), item.Description, 2, 160);
            AddPositive(result, nameof(item.Quantity), item.Quantity);
        }

        return result;
    }

    private static ValidationResult ValidateHeader(Guid organisationId, Guid branchId, Guid supplierId, string number, int itemCount)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(organisationId), organisationId);
        AddRequiredId(result, nameof(branchId), branchId);
        AddRequiredId(result, nameof(supplierId), supplierId);
        AddRequired(result, "number", number, 2, 40);
        if (itemCount == 0)
        {
            result.Add("items", "At least one item is required.");
        }

        return result;
    }

    private static void ValidateLines(ValidationResult result, IReadOnlyCollection<PurchaseLineRequest> items)
    {
        foreach (var item in items)
        {
            AddRequiredId(result, nameof(item.ProductId), item.ProductId);
            AddRequired(result, nameof(item.Description), item.Description, 2, 160);
            AddPositive(result, nameof(item.Quantity), item.Quantity);
            AddNonNegative(result, nameof(item.UnitCost), item.UnitCost);
            AddNonNegative(result, nameof(item.TaxAmount), item.TaxAmount);
        }
    }

    private static void AddRequired(ValidationResult result, string field, string? value, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(field, "Value is required.");
            return;
        }

        var length = value.Trim().Length;
        if (length < minLength || length > maxLength)
        {
            result.Add(field, $"Value must be between {minLength} and {maxLength} characters.");
        }
    }

    private static void AddRequiredId(ValidationResult result, string field, Guid value)
    {
        if (value == Guid.Empty)
        {
            result.Add(field, "Value is required.");
        }
    }

    private static void AddPositive(ValidationResult result, string field, decimal value)
    {
        if (value <= 0)
        {
            result.Add(field, "Value must be greater than zero.");
        }
    }

    private static void AddNonNegative(ValidationResult result, string field, decimal value)
    {
        if (value < 0)
        {
            result.Add(field, "Value cannot be negative.");
        }
    }
}
