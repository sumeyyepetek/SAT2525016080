using Attributes;

namespace Enums;

public enum Operation
{
    [Color("info")]
    [DisplayText("Liste")]
    [Symbol("📋")]
    List,
    [Color("success"), DisplayText("Ekle")]
    [Symbol("➕")]
    Insert,
    [Color("warning"), DisplayText("Güncelle")]
    [Symbol("✏️")]
    Update,
    [Color("danger"), DisplayText("Sil")]
    [Symbol("🗑️")]
    Delete,
    [Color("secondary"), DisplayText("İptal")]
    [Symbol("❌")]
    Cancel,
}
