namespace Base.Application.Common;

/// <summary>
/// REPRESENTS A PAGINATED RESULT SET WITH METADATA.
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// GETS OR SETS THE COLLECTION OF ITEMS FOR THE CURRENT PAGE.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// GETS OR SETS THE CURRENT PAGE NUMBER.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// GETS OR SETS THE NUMBER OF ITEMS PER PAGE.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// GETS OR SETS THE TOTAL NUMBER OF ITEMS ACROSS ALL PAGES.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// GETS THE TOTAL NUMBER OF PAGES BASED ON TOTAL COUNT AND PAGE SIZE.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// GETS A VALUE INDICATING WHETHER A PREVIOUS PAGE EXISTS.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// GETS A VALUE INDICATING WHETHER A NEXT PAGE EXISTS.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// CREATES A PAGINATED RESULT FROM THE REQUEST AND TOTAL COUNT.
    /// </summary>
    public static PagedResult<T> Create(List<T> items, PagedRequest request, int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}