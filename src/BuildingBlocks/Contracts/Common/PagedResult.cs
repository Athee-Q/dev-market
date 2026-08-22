namespace ECommerce.Contracts.Common;

/// <summary>Generic paged list envelope shared across list endpoints (Products, Orders, ...).</summary>
public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
