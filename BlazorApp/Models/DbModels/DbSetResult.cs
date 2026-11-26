namespace DbModels;

public sealed class DbSetResult
{
    public int OperationType { get; set; }
    public int AffectedCount { get; set; }
    public string? Message { get; set; }
}