using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using DbModels;
using UnitOfWorks;

namespace DbProviders;

public sealed class DbProvider<T>(IUnitOfWork unitOfWork) : IDbProvider<T>
{
    #region fields
    // Yordamların beklediği JSON alan adlarını büyük/küçük harfleriyle korur.
    private static readonly JsonSerializerOptions JsonOptions = new();

    // Kaydetme işlemine uygun basit türdeki özellikleri tutar.
    private static readonly PropertyInfo[] ScalarProperties = typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(property => property.CanRead && property.GetIndexParameters().Length == 0
            && IsScalar(property.PropertyType))
        .ToArray();
    #endregion

    // Kayıtları yükler ve işlem durumunu günceller.
    public async Task GetItemsAsync(IDbModel<T> model)
    {
        model.IsBusy = true;
        try
        {
            await ReadItemsAsync(model);

            model.IsError = false;
            model.Message = null;
        }
        catch (Exception ex)
        {
            model.IsError = true;
            model.Message = ex.Message;
        }
        finally { model.IsBusy = false; }
    }

    // Değişiklikleri kaydeder ve başarılı işlemden sonra kayıtları yeniler.
    public async Task SetItemsAsync(IDbModel<T> model)
    {
        var changes = model.Items.Where(row => row.Operation.HasValue).ToArray();

        if (changes.Length == 0) return;

        model.IsBusy = true;
        try
        {
            if (string.IsNullOrWhiteSpace(model.SetProcedure))
                throw new InvalidOperationException("SetProcedure tanımlanmadı.");

            var values = new List<JsonObject>();
            foreach (var change in changes)
            {
                var value = model.SetValue is null
                    ? CreateScalarValue(change.Item)
                    : model.SetValue(change.Item);
                var data = JsonSerializer.SerializeToNode(value, JsonOptions) as JsonObject
                    ?? throw new InvalidOperationException("İşlem verisi JSON nesnesi olmalıdır.");
                data["OperationType"] = (int)change.Operation!.Value;
                values.Add(data);
            }

            using var table = await unitOfWork.ExecuteAsync(model.SetProcedure,
                [("@Values", JsonSerializer.Serialize(values))]);
            var result = Results<DbSetResult>(table);

            model.IsError = result.Any(row =>
                row.Message?.StartsWith("Hata:", StringComparison.OrdinalIgnoreCase) == true);

            model.Message = string.Join(" | ", result.Select(row =>
                $"{row.OperationType}: {row.AffectedCount} {row.Message}"));

            //set işleminden sonra, modeli yeniden okumak için
            if (!model.IsError) await ReadItemsAsync(model);
        }
        catch (Exception ex)
        {
            model.IsError = true;
            model.Message = ex.Message;
        }
        finally { model.IsBusy = false; }
    }

    #region yardımcı metotlar
    // Filtre ve sayfalama parametreleriyle kayıtları ve sayfalama bilgilerini yükler.
    private async Task ReadItemsAsync(IDbModel<T> model)
    {
        if (string.IsNullOrWhiteSpace(model.GetProcedure))
            throw new InvalidOperationException("GetProcedure tanımlanmadı.");

        var pagination = model.Pagination;

        //DbModel'in Pagination ve Where özellikleri json formatında serileştirilir
        (string Name, string Value)[] parameters =
        [
            ("@Pagination", JsonSerializer.Serialize(model.Pagination)),
            ("@Where", JsonSerializer.Serialize(model.Where))
        ];

        using var table = await unitOfWork.ExecuteAsync(model.GetProcedure, parameters);

        var items = Results<T>(table);

        pagination.TotalRecordCount =
            table.Rows.Count == 0 ? 0
            : Convert.ToInt32(table.Rows[0]["TotalRecordCount"], CultureInfo.InvariantCulture);

        pagination.TotalPageCount = table.Rows.Count == 0 ? 0
            : Convert.ToInt32(table.Rows[0]["TotalPageCount"], CultureInfo.InvariantCulture);

        pagination.PageNumber = pagination.TotalPageCount == 0
            ? 1 : Math.Min(pagination.PageNumber, pagination.TotalPageCount);

        model.Items = items.Select(item => new DbItem<T>(item)).ToList();
    }

    // Tablo satırlarını belirtilen türdeki nesnelere dönüştürür.
    private static IReadOnlyList<TResult> Results<TResult>(DataTable table)
    {
        // Sütun ve özellik eşleştirmesi her sorguda bir kez hazırlanır.
        var columns = new List<(int Ordinal, Type Type, Action<TResult, object?> Set)>();
        foreach (DataColumn column in table.Columns)
        {
            if (TypeCache<TResult>.Setters.TryGetValue(column.ColumnName, out var setter))
                columns.Add((column.Ordinal, setter.Type, setter.Set));
        }

        var result = new List<TResult>();
        foreach (DataRow row in table.Rows)
        {
            var item = Activator.CreateInstance<TResult>();
            foreach (var column in columns)
            {
                if (row.IsNull(column.Ordinal)) continue;
                var value = row[column.Ordinal];
                if (!column.Type.IsInstanceOfType(value))
                    value = column.Type.IsEnum
                        ? Enum.ToObject(column.Type, value)
                        : Convert.ChangeType(value, column.Type, CultureInfo.InvariantCulture);
                column.Set(item, value);
            }
            result.Add(item);
        }

        return result;
    }

    // Kaydın basit türdeki özelliklerinden kaydetme verisi oluşturur.
    private static object CreateScalarValue(T item)
    {
        var data = new Dictionary<string, object?>();
        foreach (var property in ScalarProperties)
            data[property.Name] = property.GetValue(item);
        return data;
    }

    // Türün basit bir veri türü olup olmadığını belirler.
    private static bool IsScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
            || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid)
            || type == typeof(DateOnly) || type == typeof(TimeOnly);
    }
    #endregion

    #region Yardımcı sınıf : TypeCache
    private static class TypeCache<TResult>
    {
        // Özellik türlerini ve hızlı atama metotlarını önbellekte tutar.
        public static readonly Dictionary<string, (Type Type, Action<TResult, object?> Set)> Setters =
            typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.SetMethod?.IsPublic == true
                    && property.GetIndexParameters().Length == 0)
                .ToDictionary(property => property.Name,
                    property => (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType,
                        CompileSetter(property)), StringComparer.OrdinalIgnoreCase);

        // Özelliğe doğrudan değer atayan bir metot oluşturur.
        private static Action<TResult, object?> CompileSetter(PropertyInfo property)
        {
            var target = Expression.Parameter(typeof(TResult), "target");
            var value = Expression.Parameter(typeof(object), "value");
            var assign = Expression.Assign(Expression.Property(target, property),
                Expression.Convert(value, property.PropertyType));
            return Expression.Lambda<Action<TResult, object?>>(assign, target, value).Compile();
        }
    }
    #endregion
}