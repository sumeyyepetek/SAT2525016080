using System.ComponentModel.DataAnnotations;

namespace Logging;

public sealed class LogEntryTrigger
{
    public long Id { get; set; }
    [Display(Name = "Zaman")]
    public DateTime Timestamp { get; set; }
    [Display(Name = "Tablo"), MaxLength(128)]
    public string TableName { get; set; } = "";
    [Display(Name = "İşlem"), MaxLength(6)]
    public string Operation { get; set; } = "";
    [Display(Name = "Eski Değer")]
    public string? OldValue { get; set; }
    [Display(Name = "Yeni Değer")]
    public string? NewValue { get; set; }
}
