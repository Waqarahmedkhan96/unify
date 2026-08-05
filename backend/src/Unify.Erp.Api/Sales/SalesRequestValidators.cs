using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Sales;

namespace Unify.Erp.Api.Sales;

public static class SalesRequestValidators
{
    public static ValidationResult Validate(this CreateSaleRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequiredId(result, nameof(request.BranchId), request.BranchId);
        AddRequiredId(result, nameof(request.WarehouseId), request.WarehouseId);
        AddRequiredId(result, nameof(request.CustomerId), request.CustomerId);
        AddRequired(result, nameof(request.InvoiceNumber), request.InvoiceNumber, 2, 40);
        if (request.Items.Count == 0)
        {
            result.Add(nameof(request.Items), "At least one item is required.");
        }

        foreach (var item in request.Items)
        {
            AddRequiredId(result, nameof(item.ProductId), item.ProductId);
            AddRequired(result, nameof(item.Description), item.Description, 2, 160);
            AddPositive(result, nameof(item.Quantity), item.Quantity);
            AddNonNegative(result, nameof(item.UnitPrice), item.UnitPrice);
            AddNonNegative(result, nameof(item.DiscountAmount), item.DiscountAmount);
            AddNonNegative(result, nameof(item.TaxAmount), item.TaxAmount);
        }

        return result;
    }

    private static void AddRequired(ValidationResult result, string field, string? value, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(field, "Value is required.");
            return;
        }

        var trimmedLength = value.Trim().Length;
        if (trimmedLength < minLength || trimmedLength > maxLength)
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
