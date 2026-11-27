using DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Logging;

public sealed class LogReader(FileLogger fileLogger, IDbContextFactory<XDbContext> contextFactory)
{
    public async Task<List<LogEntry>> ReadFileAsync(DateTime from, DateTime to)
    {
        var range = LogDates.UtcRange(from, to);
        var items = await fileLogger.ReadAsync(range.From, range.To);
        foreach (var item in items) item.Timestamp = LogDates.ToLocal(item.Timestamp);
        return items;
    }

    public async Task<List<LogEntryDb>> ReadDbAsync(DateTime from, DateTime to)
    {
        var range = LogDates.UtcRange(from, to);
        await using var db = await contextFactory.CreateDbContextAsync();
        var items = await db.Logs.AsNoTracking()
            .Where(item => item.Timestamp >= range.From && item.Timestamp < range.To)
            .OrderByDescending(item => item.Timestamp).ThenByDescending(item => item.Id).ToListAsync();
        foreach (var item in items) item.Timestamp = LogDates.ToLocal(item.Timestamp);
        return items;
    }

    public async Task<List<LogEntryTrigger>> ReadTriggerAsync(DateTime from, DateTime to)
    {
        var range = LogDates.UtcRange(from, to);
        await using var db = await contextFactory.CreateDbContextAsync();
        var items = await db.LogsTrigger.AsNoTracking()
            .Where(item => item.Timestamp >= range.From && item.Timestamp < range.To)
            .OrderByDescending(item => item.Timestamp).ThenByDescending(item => item.Id).ToListAsync();
        foreach (var item in items) item.Timestamp = LogDates.ToLocal(item.Timestamp);
        return items;
    }
}
