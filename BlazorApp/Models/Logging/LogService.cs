namespace Logging;

public sealed class LogService(FileLogger fileLogger, DbLogger dbLogger, ILogger<LogService> logger)
{
    public async Task WriteAsync(string operation, string message, bool isError = false)
    {
        var entry = new LogEntry
        {
            Level = isError ? "Error" : "Information",
            Operation = operation,
            Message = message
        };

        // Bir log hedefindeki hata diğer hedefe yazmayı ve bölüm işlemini engellemez.
        try { await fileLogger.WriteAsync(entry); }
        catch (Exception ex) { logger.LogError(ex, "FileLogger yazma hatası"); }

        try { await dbLogger.WriteAsync(entry); }
        catch (Exception ex)
        {
            logger.LogError(ex, "DbLogger yazma hatası");
            try
            {
                await fileLogger.WriteAsync(new LogEntry
                {
                    Level = "Error", Operation = "DbLogger", Message = ex.Message
                });
            }
            catch (Exception fileError) { logger.LogError(fileError, "FileLogger yazma hatası"); }
        }
    }
}
