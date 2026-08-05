using Unify.Erp.Application.Common;

namespace Unify.Erp.Infrastructure.Common;

public sealed class SystemExecutionContext : IExecutionContext
{
    public Guid? UserId => null;

    public string? UserEmail => null;

    public string? CorrelationId => null;
}
