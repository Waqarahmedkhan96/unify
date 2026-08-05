using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Products;

public sealed class UnitOfMeasure : TenantEntity
{
    public UnitOfMeasure(Guid id, Guid organisationId, string code, string name, int decimalPlaces)
        : base(id, organisationId)
    {
        Code = Guard.RequiredText(code, nameof(code), 16).ToUpperInvariant();
        Name = Guard.RequiredText(name, nameof(name), 80);
        DecimalPlaces = Guard.Range(decimalPlaces, nameof(decimalPlaces), 0, 6);
    }

    public string Code { get; }

    public string Name { get; }

    public int DecimalPlaces { get; }
}
