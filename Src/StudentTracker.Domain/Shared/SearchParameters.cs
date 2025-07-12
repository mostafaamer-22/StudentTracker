
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Shared;
public class SearchParameters
{
    public SearchParameters()
    {

    }
    public string? SearchText { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Newest;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public SearchParameters(string? searchText, SortOrder sortOrder, int page, int pageSize)
    {
        if (searchText != null)
            SearchText = searchText;
        SortOrder = sortOrder;
        Page = page;
        PageSize = pageSize;
    }
}
