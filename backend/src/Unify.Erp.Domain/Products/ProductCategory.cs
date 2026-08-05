using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Products;

public sealed class ProductCategory : TenantEntity
{
    public ProductCategory(Guid id, Guid organisationId, string code, string name)
        : base(id, organisationId)
    {
        Code = Guard.RequiredText(code, nameof(code), 32).ToUpperInvariant();
        Name = Guard.RequiredText(name, nameof(name), 120);
    }

    public string Code { get; }

    public string Name { get; }
}
