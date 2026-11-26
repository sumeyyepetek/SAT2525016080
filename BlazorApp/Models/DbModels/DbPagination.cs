namespace DbModels;

public sealed class DbPagination
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string OrderBy { get; set; } = "Id ASC";
    public int TotalRecordCount { get; set; }
    public int TotalPageCount { get; set; }
}