using System.Reflection;
using Attributes;

namespace Extensions;

public static class EnumExtensions
{

    // Metotlar
    public static T? GetAttr<T>(this Enum item)
        where T : Attribute => item.GetType().GetField(item.ToString())?.GetCustomAttribute<T>();
    public static T? GetAttribute<T>(this Enum field)
        where T : Attribute => field.GetType().GetField(field.ToString())?.GetCustomAttribute<T>();
    public static string Color(this Enum field) => field.GetAttribute<ColorAttribute>()?.Value ?? "";
    public static string DisplayText(this Enum field) => field.GetAttribute<DisplayTextAttribute>()?.Value ?? field.ToString();
    public static string Symbol(this Enum field) => field.GetAttribute<SymbolAttribute>()?.Value ?? field.ToString();
    public static string SymbolDisplayText(this Enum field) => $"{field.Symbol()} {field.DisplayText()}";

    public static int ToInt<TEnum>(this TEnum value)
        where TEnum : struct, Enum => Convert.ToInt32(value);
    public static TEnum ToEnum<TEnum>(this int value)
        where TEnum : struct, Enum => (TEnum)Enum.ToObject(typeof(TEnum), value);
    public static TEnum ToEnum<TEnum>(this byte value)
        where TEnum : struct, Enum => ((int)value).ToEnum<TEnum>();
}
