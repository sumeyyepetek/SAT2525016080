using Attributes;
 
namespace Enums;
 
public enum UIAction
{
    [Color("success"), DisplayText("Yeni"), Symbol("➕")]
    New,
 
    [Color("secondary"), DisplayText("İptal"), Symbol("❌")]
    Cancel,
 
    [Color("primary"), DisplayText("Kaydet"), Symbol("💾")]
    Save,
 
    [Color("info"), DisplayText("Liste"), Symbol("📋")]
    List,
 
    [Color("primary"), DisplayText("Filtrele"), Symbol("🔍")]
    Filter,
 
    [DisplayText("Tümünü seç"), Symbol("☑")]
    SelectAll,
 
    [DisplayText("Seçimi kaldır"), Symbol("☐")]
    ClearSelection,
 
    [DisplayText("Seçimi ters çevir"), Symbol("⇄")]
    InvertSelection,
 
    [Color("secondary"), DisplayText("Kapat"), Symbol("✖")]
    Close
}