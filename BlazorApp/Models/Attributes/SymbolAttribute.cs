namespace Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class SymbolAttribute(string value) : Attribute
{

    // Özellikler
    public string Value { get; set; } = value;
}
