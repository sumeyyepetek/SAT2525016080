using System.Data;
using Microsoft.Data.SqlClient;

namespace UnitOfWorks;

public sealed class UnitOfWork(string connectionString) : IUnitOfWork
{
    // Bağlantı bilgisiyle UnitOfWork örneği oluşturur.
    public static UnitOfWork Create(string connectionString)
        => new(connectionString);

    // Yordamı parametreleriyle çalıştırır ve sonucu tablo olarak döndürür.
    public async Task<DataTable> ExecuteAsync(
        string procedure,
        (string Name, string Value)[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(procedure, connection)
            { CommandType = CommandType.StoredProcedure };

        foreach (var (name, value) in parameters)
            command.Parameters.Add(new SqlParameter(name, SqlDbType.NVarChar, -1) { Value = value });

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        var table = new DataTable();

        for (var i = 0; i < reader.FieldCount; i++)
            table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

        while (await reader.ReadAsync())
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            table.Rows.Add(values);
        }

        return table;
    }
}