namespace Unify.Erp.Application.Common;

public interface IExecutionContext
{
    Guid? UserId { get; }

    string? UserEmail { get; }

    string? CorrelationId { get; }
}
