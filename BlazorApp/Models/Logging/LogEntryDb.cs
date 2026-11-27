using System.ComponentModel.DataAnnotations;

namespace Logging;

public sealed class LogEntryDb
{
    public long Id { get; set; }
    [Display(Name = "Zaman")]
    public DateTime Timestamp { get; set; }
    [Display(Name = "Seviye"), MaxLength(20)]
    public string Level { get; set; } = "";
    [Display(Name = "Kategori"), MaxLength(100)]
    public string Category { get; set; } = "";
    [Display(Name = "İşlem"), MaxLength(20)]
    public string Operation { get; set; } = "";
    [Display(Name = "Mesaj")]
    public string Message { get; set; } = "";
}
