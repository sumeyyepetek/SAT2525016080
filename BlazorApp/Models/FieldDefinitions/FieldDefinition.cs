using System.ComponentModel.DataAnnotations;
using System.Reflection;
 
namespace FieldDefinitions;
 
public sealed record FieldDefinition(
    string Name,
    string Label,
    string Placeholder = "",
    bool Filterable = true,
    bool Sortable = true,
    IReadOnlyList<FieldOption>? Options = null)
{
    public static IReadOnlyList<FieldDefinition> From<T>() => typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(property => property.CanRead && property.GetIndexParameters().Length == 0
            && IsScalar(property.PropertyType))
        .Select(property => new FieldDefinition(property.Name,
            property.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? property.Name))
        .ToArray();
 
    public static bool IsScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
            || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid)
            || type == typeof(DateOnly) || type == typeof(TimeOnly);
    }
}