namespace DbModels;

public sealed class DbFilter()
    : Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    //indeksleyici (indexer)
    public new string this[string key]
    {
        get => TryGetValue(key, out var value) ? value : "";
        set
        {
            if (string.IsNullOrWhiteSpace(value)) Remove(key);
            else base[key] = value;
        }
    }
}