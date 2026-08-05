using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Products;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Products;
using Unify.Erp.Domain.Products;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Products;

public sealed class ProductCatalogService : IProductCatalogService
{
    private readonly ApplicationDbContext _dbContext;

    public ProductCatalogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UnitOfMeasureResponse> CreateUnitOfMeasureAsync(
        CreateUnitOfMeasureRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureOrganisationExistsAsync(request.OrganisationId, cancellationToken);
        await EnsureUniqueAsync(
            _dbContext.UnitsOfMeasure.AnyAsync(
                unit => unit.OrganisationId == request.OrganisationId
                    && unit.Code == request.Code.Trim().ToUpperInvariant(),
                cancellationToken),
            "Unit of measure code already exists for the organisation.");

        var unit = new UnitOfMeasure(Guid.NewGuid(), request.OrganisationId, request.Code, request.Name, request.DecimalPlaces);
        _dbContext.UnitsOfMeasure.Add(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(unit);
    }

    public async Task<IReadOnlyCollection<UnitOfMeasureResponse>> ListUnitsOfMeasureAsync(
        Guid organisationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UnitsOfMeasure
            .AsNoTracking()
            .Where(unit => unit.OrganisationId == organisationId)
            .OrderBy(unit => unit.Code)
            .Select(unit => ToResponse(unit))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductCategoryResponse> CreateCategoryAsync(
        CreateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureOrganisationExistsAsync(request.OrganisationId, cancellationToken);
        await EnsureUniqueAsync(
            _dbContext.ProductCategories.AnyAsync(
                category => category.OrganisationId == request.OrganisationId
                    && category.Code == request.Code.Trim().ToUpperInvariant(),
                cancellationToken),
            "Product category code already exists for the organisation.");

        var category = new ProductCategory(Guid.NewGuid(), request.OrganisationId, request.Code, request.Name);
        _dbContext.ProductCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(category);
    }

    public async Task<IReadOnlyCollection<ProductCategoryResponse>> ListCategoriesAsync(
        Guid organisationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProductCategories
            .AsNoTracking()
            .Where(category => category.OrganisationId == organisationId)
            .OrderBy(category => category.Code)
            .Select(category => ToResponse(category))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductResponse> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureOrganisationExistsAsync(request.OrganisationId, cancellationToken);
        await EnsureUnitExistsAsync(request.OrganisationId, request.UnitOfMeasureId, cancellationToken);
        if (request.CategoryId.HasValue)
        {
            await EnsureCategoryExistsAsync(request.OrganisationId, request.CategoryId.Value, cancellationToken);
        }

        await EnsureUniqueAsync(
            _dbContext.Products.AnyAsync(
                product => product.OrganisationId == request.OrganisationId
                    && product.ProductCode == request.ProductCode.Trim().ToUpperInvariant(),
                cancellationToken),
            "Product code already exists for the organisation.");

        var product = new Product(
            Guid.NewGuid(),
            request.OrganisationId,
            request.UnitOfMeasureId,
            request.CategoryId,
            request.ProductCode,
            request.Name,
            request.Barcode,
            request.PurchasePrice,
            request.SalesPrice,
            request.IsInventoryTracked);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse?> GetProductAsync(
        Guid organisationId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.OrganisationId == organisationId && product.Id == productId)
            .Select(product => ToResponse(product))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<ProductResponse>> ListProductsAsync(
        ListProductsRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.OrganisationId == request.OrganisationId);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(product =>
                product.ProductCode.Contains(search)
                || product.Name.ToUpper().Contains(search)
                || (product.Barcode != null && product.Barcode.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(product => product.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(product => ToResponse(product))
            .ToListAsync(cancellationToken);

        return new PagedResponse<ProductResponse>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<bool> DeactivateProductAsync(
        Guid organisationId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .SingleOrDefaultAsync(
                product => product.OrganisationId == organisationId && product.Id == productId,
                cancellationToken);

        if (product is null)
        {
            return false;
        }

        product.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureOrganisationExistsAsync(Guid organisationId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Organisations.AnyAsync(
            organisation => organisation.Id == organisationId,
            cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException("Organisation does not exist.");
        }
    }

    private async Task EnsureUnitExistsAsync(Guid organisationId, Guid unitId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.UnitsOfMeasure.AnyAsync(
            unit => unit.OrganisationId == organisationId && unit.Id == unitId,
            cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException("Unit of measure does not exist for the organisation.");
        }
    }

    private async Task EnsureCategoryExistsAsync(Guid organisationId, Guid categoryId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ProductCategories.AnyAsync(
            category => category.OrganisationId == organisationId && category.Id == categoryId,
            cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException("Product category does not exist for the organisation.");
        }
    }

    private static async Task EnsureUniqueAsync(Task<bool> duplicateExistsTask, string message)
    {
        if (await duplicateExistsTask)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static UnitOfMeasureResponse ToResponse(UnitOfMeasure unit)
    {
        return new UnitOfMeasureResponse(unit.Id, unit.OrganisationId, unit.Code, unit.Name, unit.DecimalPlaces);
    }

    private static ProductCategoryResponse ToResponse(ProductCategory category)
    {
        return new ProductCategoryResponse(category.Id, category.OrganisationId, category.Code, category.Name);
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.OrganisationId,
            product.UnitOfMeasureId,
            product.CategoryId,
            product.ProductCode,
            product.Name,
            product.Barcode,
            product.PurchasePrice,
            product.SalesPrice,
            product.IsInventoryTracked,
            product.Status.ToString(),
            product.CreatedAtUtc);
    }
}
