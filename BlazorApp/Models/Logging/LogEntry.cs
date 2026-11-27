using System.ComponentModel.DataAnnotations;

namespace Logging;

public sealed class LogEntry
{
    [Display(Name = "Zaman")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [Display(Name = "Seviye")]
    public string Level { get; init; } = "Information";
    [Display(Name = "Kategori")]
    public string Category { get; init; } = "Departments_Logs";
    [Display(Name = "İşlem")]
    public string Operation { get; init; } = "";
    [Display(Name = "Mesaj")]
    public string Message { get; init; } = "";
}