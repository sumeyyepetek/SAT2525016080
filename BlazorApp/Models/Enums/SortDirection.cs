using Attributes;
 
namespace Enums;
 
public enum SortDirection
{
    [DisplayText("Artan"), Symbol("▲")]
    ASC,
 
    [DisplayText("Azalan"), Symbol("▼")]
    DESC
}