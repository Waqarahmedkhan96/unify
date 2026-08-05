namespace Unify.Erp.Api.Common;

public sealed class HttpsOptions
{
    public const string SectionName = "Https";

    public bool RequireHttps { get; init; } = true;

    public int HstsDays { get; init; } = 365;
}
