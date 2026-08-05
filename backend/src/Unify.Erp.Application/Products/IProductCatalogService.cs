using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Products;

namespace Unify.Erp.Application.Products;

public interface IProductCatalogService
{
    Task<UnitOfMeasureResponse> CreateUnitOfMeasureAsync(
        CreateUnitOfMeasureRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UnitOfMeasureResponse>> ListUnitsOfMeasureAsync(
        Guid organisationId,
        CancellationToken cancellationToken);

    Task<ProductCategoryResponse> CreateCategoryAsync(
        CreateProductCategoryRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProductCategoryResponse>> ListCategoriesAsync(
        Guid organisationId,
        CancellationToken cancellationToken);

    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductResponse?> GetProductAsync(Guid organisationId, Guid productId, CancellationToken cancellationToken);

    Task<PagedResponse<ProductResponse>> ListProductsAsync(ListProductsRequest request, CancellationToken cancellationToken);

    Task<bool> DeactivateProductAsync(Guid organisationId, Guid productId, CancellationToken cancellationToken);
}
