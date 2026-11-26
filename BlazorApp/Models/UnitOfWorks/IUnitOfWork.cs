using System.Data;

namespace UnitOfWorks;

public interface IUnitOfWork
{
    Task<DataTable> ExecuteAsync(
        string procedure,
        (string Name, string Value)[] parameters);
}


/*

GET : (Veri Alma İşlemleri)
Procedure: School.Sp_Get_Departments

- UI (input,select,button,...)
- DbModel (ProcedureName, DbPagination, DbFilter)
- DbProvider (DbPagination:  Pagination => JSON, DbFilter: Where => JSON)
- UnitOfWork (Execute)
- DbProvider (DbItem, DbPagination)
- DbModel (DbItem, DbPagination)
- UI (List, Table)


SET : (Veri Ekleme, Güncelleme, Silme İşlemleri)
Procedure: School.Sp_Set_Departments

- UI (input,select,button,...)
- DbModel (ProcedureName, DbItem)
- DbProvider (DbItem => JSON : Values)
- UnitOfWork (Execute)
- DbProvider (DbSetResult)
- DbModel (DbSetResult)
- UI (Message)

----

UI -> DbModel -> DbProvider -> UnitOfWork -> DbProvider -> DbModel -> UI
Get: Pagination+Where => JSON
Set: List<DbItem<T>> => JSON

Get: DataTable -> List<DbItem<T>>
Set: DatabTable -> List<DbSetResult>


*/