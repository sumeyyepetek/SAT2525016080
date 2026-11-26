namespace DbModels;

public interface IDbModel<T>
{
    List<DbItem<T>> Items { get; set; }
    DbPagination Pagination { get; }
    DbFilter Where { get; }
    string GetProcedure { get; set; }
    string SetProcedure { get; set; }
    Func<T, object>? SetValue { get; set; }
    bool IsBusy { get; set; }
    bool IsError { get; set; }
    string? Message { get; set; }
    bool HasSelectedExisting { get; }
    bool HasSelection { get; }
    bool HasChanges { get; }
}