namespace Unify.Erp.Contracts.Common;

public sealed record PagedRequest(int PageNumber = 1, int PageSize = 50)
{
    public const int MaxPageSize = 200;

    public int NormalizedPageNumber => PageNumber < 1 ? 1 : PageNumber;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 50,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };
}
