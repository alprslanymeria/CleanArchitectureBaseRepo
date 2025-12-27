namespace Base.Application.Common;

/// <summary>
/// REPRESENTS A PAGINATION REQUEST WITH PAGE NUMBER AND SIZE.
/// </summary>
public class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 10;

    /// <summary>
    /// PAGE NUMBER (1-BASED). DEFAULTS TO 1.
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// NUMBER OF ITEMS PER PAGE. DEFAULTS TO 10, MAX 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
    }
}