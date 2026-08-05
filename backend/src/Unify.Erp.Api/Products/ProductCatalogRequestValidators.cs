using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Products;

namespace Unify.Erp.Api.Products;

public static class ProductCatalogRequestValidators
{
    public static ValidationResult Validate(this CreateUnitOfMeasureRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequired(result, nameof(request.Code), request.Code, 1, 16);
        AddRequired(result, nameof(request.Name), request.Name, 2, 80);
        if (request.DecimalPlaces is < 0 or > 6)
        {
            result.Add(nameof(request.DecimalPlaces), "Value must be between 0 and 6.");
        }

        return result;
    }

    public static ValidationResult Validate(this CreateProductCategoryRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequired(result, nameof(request.Code), request.Code, 2, 32);
        AddRequired(result, nameof(request.Name), request.Name, 2, 120);

        return result;
    }

    public static ValidationResult Validate(this CreateProductRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequiredId(result, nameof(request.UnitOfMeasureId), request.UnitOfMeasureId);
        AddRequired(result, nameof(request.ProductCode), request.ProductCode, 2, 32);
        AddRequired(result, nameof(request.Name), request.Name, 2, 160);
        AddOptional(result, nameof(request.Barcode), request.Barcode, 80);
        if (request.PurchasePrice < 0)
        {
            result.Add(nameof(request.PurchasePrice), "Value cannot be negative.");
        }

        if (request.SalesPrice < 0)
        {
            result.Add(nameof(request.SalesPrice), "Value cannot be negative.");
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

    private static void AddOptional(ValidationResult result, string field, string? value, int maxLength)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
        {
            result.Add(field, $"Value cannot exceed {maxLength} characters.");
        }
    }

    private static void AddRequiredId(ValidationResult result, string field, Guid value)
    {
        if (value == Guid.Empty)
        {
            result.Add(field, "Value is required.");
        }
    }
}
