using System.Data;
using Microsoft.Data.SqlClient;

namespace Logging;

public sealed class DbLogger(IConfiguration configuration)
{
    public async Task WriteAsync(LogEntry entry)
    {
        await using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Logs ([Timestamp], [Level], Category, Operation, Message)
            VALUES (@Timestamp, @Level, @Category, @Operation, @Message);
            """;
        command.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = entry.Timestamp;
        command.Parameters.Add("@Level", SqlDbType.NVarChar, 20).Value = entry.Level;
        command.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = entry.Category;
        command.Parameters.Add("@Operation", SqlDbType.NVarChar, 20).Value = entry.Operation;
        command.Parameters.Add("@Message", SqlDbType.NVarChar, -1).Value = entry.Message;

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
}
