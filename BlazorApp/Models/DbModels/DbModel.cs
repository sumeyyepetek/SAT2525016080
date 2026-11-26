namespace DbModels;

public sealed class DbModel<T> : IDbModel<T>
{
    // Kayıtları ve her kaydın işlem durumunu tutar.
    public List<DbItem<T>> Items { get; set; } = [];

    // Sayfalama ve sıralama bilgilerini tutar.
    public DbPagination Pagination { get; } = new();

    // Sorguda kullanılacak filtreleri tutar.
    public DbFilter Where { get; } = new();

    // Kayıtları getiren yordamın adını tutar.
    public string GetProcedure { get; set; } = "";

    // Değişiklikleri kaydeden yordamın adını tutar.
    public string SetProcedure { get; set; } = "";

    // Kaydetme verisini hazırlayan özel dönüşüm fonksiyonunu tutar.
    public Func<T, object>? SetValue { get; set; }

    // Bir işlemin sürüp sürmediğini belirtir.
    public bool IsBusy { get; set; }

    // İşlemde hata oluşup oluşmadığını belirtir.
    public bool IsError { get; set; }

    // İşlem sonucuna veya hataya ait mesajı tutar.
    public string? Message { get; set; }

    // Seçili kayıtlar arasında mevcut bir kayıt bulunup bulunmadığını belirtir.
    public bool HasSelectedExisting => Items.Any(row => row.Selected && row.Original is not null);

    // Seçili kayıt bulunup bulunmadığını belirtir.
    public bool HasSelection => Items.Any(row => row.Selected);

    // Bekleyen kayıt değişikliği bulunup bulunmadığını belirtir.
    public bool HasChanges => Items.Any(row => row.Operation.HasValue);
}