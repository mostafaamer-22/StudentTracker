namespace StudentTracker.Domain.Shared;
public sealed class Pagination<TData> : Result<IReadOnlyList<TData>>
{
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalItems { get; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public Pagination(int currentPage, int pageSize, int totalItems, IReadOnlyList<TData> data)
        : base(data, true, Error.None)
    {
        if (currentPage <= 0) throw new ArgumentOutOfRangeException(nameof(currentPage), "Current page must be greater than 0.");
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
        if (totalItems < 0) throw new ArgumentOutOfRangeException(nameof(totalItems), "Total items cannot be negative.");

        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public static Pagination<TData> Success(int currentPage, int pageSize, int totalItems, IReadOnlyList<TData> data)
    {
        return new Pagination<TData>(currentPage, pageSize, totalItems, data);
    }
}
