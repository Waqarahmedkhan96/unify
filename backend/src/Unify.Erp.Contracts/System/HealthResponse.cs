namespace Unify.Erp.Contracts.System;

public sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset CheckedAtUtc);
