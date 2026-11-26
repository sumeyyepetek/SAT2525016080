using Enums;

namespace DbModels;

public sealed class DbItem<T>(T item, bool isNew = false)
{
    public T Item { get; set; } = item;
    public T? Original { get; } = isNew ? default : item;
    public bool Selected { get; set; } = isNew;
    public Operation? Operation { get; set; } =
        isNew ? Enums.Operation.Insert : null;
}


/*
UnitOfWork : VT üzerinde, yordam çalıştırmak
DbProvider : DbModel üzerindeki verileri, UOW'e hazırlama veya
    UOW ile elde edilen verileri, DbModel'e aktarma

DbModel     : Verileri tutma, sayfalama, filtreleme, sıralama bilgilerini tutma
DbItem      :
    DbModel Items koleksiyonundaki veri satırını tutma;
    satırın operation ve seçim durumlarını tutma;
    satırın orjinal halini tutar, gerektiğinde orjinal haline dönüş yapılır

DbPagination: Sayfalama ve sıralama bilgileri
DbFilter    : Filtreleme (where) bilgileri
DbSetResult : I/U/D işlemleri sonrası, set yordamından etkilenen satır sayılarını ve mesajları tutma

GET:
UI (input,select, button vs.) -> DbModel (DbPagination, DbFilter) -> DbProvider -> UnitOfWork -> DbProvider -> DbModel (DbItem, DbPagination) -> UI (list, table)

SET:
UI (input,select, button vs.) -> DbModel (Values) -> DbProvider -> UnitOfWork -> DbProvider -> DbModel (DbSetResult) -> UI (message)
*/