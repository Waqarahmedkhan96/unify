using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Api.Platform;

public static class PlatformRequestValidators
{
    public static ValidationResult Validate(this CreateOrganisationRequest request)
    {
        var result = new ValidationResult();

        AddRequired(result, nameof(request.LegalName), request.LegalName, 2, 200);
        AddRequired(result, nameof(request.DisplayName), request.DisplayName, 2, 120);
        AddRequired(result, nameof(request.BaseCurrency), request.BaseCurrency, 3, 3);
        AddRequired(result, nameof(request.Timezone), request.Timezone, 3, 100);

        return result;
    }

    public static ValidationResult Validate(this CreateBranchRequest request)
    {
        var result = new ValidationResult();

        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequired(result, nameof(request.Code), request.Code, 2, 32);
        AddRequired(result, nameof(request.Name), request.Name, 2, 120);
        AddRequired(result, nameof(request.Timezone), request.Timezone, 3, 100);

        return result;
    }

    public static ValidationResult Validate(this CreateWarehouseRequest request)
    {
        var result = new ValidationResult();

        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequiredId(result, nameof(request.BranchId), request.BranchId);
        AddRequired(result, nameof(request.Code), request.Code, 2, 32);
        AddRequired(result, nameof(request.Name), request.Name, 2, 120);

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

    private static void AddRequiredId(ValidationResult result, string field, Guid value)
    {
        if (value == Guid.Empty)
        {
            result.Add(field, "Value is required.");
        }
    }
}
