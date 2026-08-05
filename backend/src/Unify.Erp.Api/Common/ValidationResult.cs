namespace Unify.Erp.Api.Common;

public sealed class ValidationResult
{
    private readonly Dictionary<string, string[]> _errors = new(StringComparer.Ordinal);

    public bool IsValid => _errors.Count == 0;

    public IDictionary<string, string[]> Errors => _errors;

    public void Add(string field, string message)
    {
        _errors[field] = [message];
    }
}
