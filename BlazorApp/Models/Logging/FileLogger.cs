using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace Logging;

public sealed class FileLogger(IWebHostEnvironment environment) : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private string DirectoryPath => Path.Combine(environment.ContentRootPath, "Logs");

    public async Task WriteAsync(LogEntry entry)
    {
        // Günlük dosya Türkiye tarihine göre seçilir; kayıt zamanı UTC olarak saklanır.
        var date = LogDates.ToLocal(entry.Timestamp);
        var directory = DirectoryPath;
        var path = Path.Combine(directory, date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".txt");
        var line = string.Join(",", new[]
        {
            entry.Timestamp.ToString("O", CultureInfo.InvariantCulture), entry.Level,
            entry.Category, entry.Operation, entry.Message
        }.Select(Csv));

        await gate.WaitAsync();
        try
        {
            Directory.CreateDirectory(directory);
            var header = !File.Exists(path) || new FileInfo(path).Length == 0
                ? "Timestamp,Level,Category,Operation,Message" + Environment.NewLine : "";
            await File.AppendAllTextAsync(path, header + line + Environment.NewLine, Encoding.UTF8);
        }
        finally { gate.Release(); }
    }

    public async Task<List<LogEntry>> ReadAsync(DateTime fromUtc, DateTime toUtc)
    {
        var items = new List<LogEntry>();
        await gate.WaitAsync();
        try
        {
            if (!Directory.Exists(DirectoryPath)) return items;

            foreach (var path in Directory.EnumerateFiles(DirectoryPath, "????????.txt").OrderBy(path => path))
            {
                if (!DateTime.TryParseExact(Path.GetFileNameWithoutExtension(path), "yyyyMMdd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                    || date < LogDates.ToLocal(fromUtc).Date || date >= LogDates.ToLocal(toUtc).Date) continue;

                var content = await File.ReadAllTextAsync(path, Encoding.UTF8);
                using var parser = new TextFieldParser(new StringReader(content))
                {
                    HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = false
                };
                parser.SetDelimiters(",");
                var header = parser.ReadFields();
                if (header is null) continue;
                if (!header.SequenceEqual(new[] { "Timestamp", "Level", "Category", "Operation", "Message" }))
                    throw new FormatException($"CSV başlığı geçersiz: {Path.GetFileName(path)}");

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (fields is null || fields.Length != 5)
                        throw new FormatException($"CSV kaydı geçersiz: {Path.GetFileName(path)}");
                    var timestamp = DateTime.Parse(fields[0], CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind).ToUniversalTime();
                    if (timestamp >= fromUtc && timestamp < toUtc)
                        items.Add(new LogEntry
                        {
                            Timestamp = timestamp, Level = fields[1], Category = fields[2],
                            Operation = fields[3], Message = fields[4]
                        });
                }
            }
        }
        finally { gate.Release(); }
        return items.OrderByDescending(item => item.Timestamp).ToList();
    }

    private static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    public void Dispose() => gate.Dispose();
}
