using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Payments;

namespace Unify.Erp.Api.Payments;

public static class PaymentRequestValidators
{
    public static ValidationResult Validate(this CreateCustomerPaymentRequest request)
    {
        var result = new ValidationResult();
        AddRequiredId(result, nameof(request.OrganisationId), request.OrganisationId);
        AddRequiredId(result, nameof(request.BranchId), request.BranchId);
        AddRequiredId(result, nameof(request.CustomerId), request.CustomerId);
        AddRequired(result, nameof(request.ReceiptNumber), request.ReceiptNumber, 2, 40);
        AddRequired(result, nameof(request.Method), request.Method, 2, 40);
        if (request.Amount <= 0)
        {
            result.Add(nameof(request.Amount), "Value must be greater than zero.");
        }

        foreach (var allocation in request.Allocations)
        {
            AddRequiredId(result, nameof(allocation.SaleId), allocation.SaleId);
            if (allocation.Amount <= 0)
            {
                result.Add(nameof(allocation.Amount), "Value must be greater than zero.");
            }
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
}
