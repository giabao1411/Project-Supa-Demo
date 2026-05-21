public class PageResult<T>
{
    public List<T> Items { get; set; } = new List<T>();

    public int TotalItems { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}