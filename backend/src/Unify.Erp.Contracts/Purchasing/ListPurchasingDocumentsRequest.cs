using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Purchasing;

public sealed record ListPurchasingDocumentsRequest(Guid OrganisationId, Guid? SupplierId, PagedRequest Page);
