using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Suppliers;

namespace Unify.Erp.Api.Suppliers;

public static class SupplierRequestValidators
{
    public static ValidationResult Validate(this CreateSupplierRequest request)
    {
        var result = new ValidationResult();

        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequired(result, nameof(request.SupplierNumber), request.SupplierNumber, 2, 32);
        AddRequired(result, nameof(request.DisplayName), request.DisplayName, 2, 160);
        AddOptional(result, nameof(request.LegalName), request.LegalName, 200);
        AddOptional(result, nameof(request.Phone), request.Phone, 40);
        AddOptional(result, nameof(request.Email), request.Email, 254);
        AddOptional(result, nameof(request.TaxNumber), request.TaxNumber, 80);

        return result;
    }

    private static void AddRequired(
        ValidationResult result,
        string field,
        string? value,
        int minLength,
        int maxLength)
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
