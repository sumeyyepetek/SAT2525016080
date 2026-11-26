using System.Globalization;
using System.Reflection;
 
namespace Helpers;
 
internal static class PropertyInput
{
    public static Type ValueType(PropertyInfo property) =>
        Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
 
    public static string InputType(PropertyInfo property)
    {
        var type = ValueType(property);
        if (type == typeof(DateOnly)) return "date";
        if (type == typeof(DateTime)) return "datetime-local";
        if (type == typeof(TimeOnly)) return "time";
        if (type == typeof(byte) || type == typeof(short) || type == typeof(int)
            || type == typeof(long) || type == typeof(float) || type == typeof(double)
            || type == typeof(decimal)) return "number";
        return "text";
    }
 
    public static string? InputStep(PropertyInfo property)
    {
        var type = ValueType(property);
        return type == typeof(float) || type == typeof(double) || type == typeof(decimal) ? "any" : null;
    }
 
    public static string InputValue<T>(T item, PropertyInfo property)
    {
        var value = property.GetValue(item);
        return value switch
        {
            DateTime date => date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeOnly time => time.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToString("O", CultureInfo.InvariantCulture),
            Enum enumValue => Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
        };
    }
 
    public static void SetValue<T>(
        T item,
        PropertyInfo property,
        object? value)
    {
        if (value is null || value is string text && text.Length == 0)
        {
            if (!property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) is not null)
                property.SetValue(item, null);
            return;
        }
 
        var type = ValueType(property);
        var textValue = value.ToString()!;
        var converted = type.IsEnum ? Enum.Parse(type, textValue)
            : type == typeof(DateOnly) ? DateOnly.Parse(textValue, CultureInfo.InvariantCulture)
            : type == typeof(TimeOnly) ? TimeOnly.Parse(textValue, CultureInfo.InvariantCulture)
            : type == typeof(DateTimeOffset) ? DateTimeOffset.Parse(textValue, CultureInfo.InvariantCulture)
            : type == typeof(Guid) ? Guid.Parse(textValue)
            : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        property.SetValue(item, converted);
    }
}