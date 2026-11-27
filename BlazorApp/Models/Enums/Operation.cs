using Attributes;
 
namespace Enums;
 
public enum Operation
{
    [Color("info")]
    [DisplayText("Liste")]
    [Symbol("📋")]
    List = 2,
    [Color("success"), DisplayText("Ekle")]
    [Symbol("➕")]
    Insert = 1,
    [Color("warning"), DisplayText("Güncelle")]
    [Symbol("✏️")]
    Update = 0,
    [Color("danger"), DisplayText("Sil")]
    [Symbol("🗑️")]
    Delete = -1,
    [Color("secondary"), DisplayText("İptal")]
    [Symbol("❌")]
    Cancel = 3,
}