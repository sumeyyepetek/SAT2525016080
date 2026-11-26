namespace Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class DisplayTextAttribute(string value) : Attribute
{

    // Özellikler
    public string Value { get; set; } = value;
}
