namespace Unify.Erp.Domain.Common;

public static class Guard
{
    public static Guid RequiredId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", parameterName);
        }

        return value;
    }

    public static string RequiredText(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return trimmed;
    }

    public static string? OptionalText(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return trimmed;
    }

    public static decimal NonNegativeMoney(decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentException("Value cannot be negative.", parameterName);
        }

        return value;
    }

    public static int Range(int value, string parameterName, int minimum, int maximum)
    {
        if (value < minimum || value > maximum)
        {
            throw new ArgumentException($"Value must be between {minimum} and {maximum}.", parameterName);
        }

        return value;
    }

    public static decimal PositiveQuantity(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Value must be greater than zero.", parameterName);
        }

        return value;
    }
}
