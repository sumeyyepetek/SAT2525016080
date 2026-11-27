-- Veritabanı
use BlazorApp;
go

-- Uygulama Log Tablosu
if object_id(N'dbo.Logs', N'U') is null
begin
    create table dbo.Logs
    (
        Id bigint identity(1,1) not null primary key,
        [Timestamp] datetime2(7) not null default sysutcdatetime(),
        [Level] nvarchar(20) not null,
        Category nvarchar(100) not null,
        Operation nvarchar(20) not null,
        Message nvarchar(max) not null
    );
end;
go

-- Trigger Log Tablosu
if object_id(N'dbo.Logs_Trigger', N'U') is null
begin
    create table dbo.Logs_Trigger
    (
        Id bigint identity(1,1) not null primary key,
        [Timestamp] datetime2(7) not null default sysutcdatetime(),
        TableName sysname not null,
        Operation varchar(6) not null,
        OldValue nvarchar(max) null,
        NewValue nvarchar(max) null
    );
end;
go

-- Çok Satırlı Tetikleyici
create or alter trigger School.TR_Department_Logs
on School.Department
after insert, update, delete
as
begin
    set nocount on;

    insert into dbo.Logs_Trigger (TableName, Operation, OldValue, NewValue)
    select N'School.Department',
        case when d.Id is null then 'INSERT'
             when i.Id is null then 'DELETE'
             else 'UPDATE' end,
        case when d.Id is not null then
            (select d.Id, d.Name, d.CreatedBy, d.CreatedDateTime
             for json path, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) end,
        case when i.Id is not null then
            (select i.Id, i.Name, i.CreatedBy, i.CreatedDateTime
             for json path, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) end
    from inserted i
        full join deleted d on d.Id = i.Id;
end;
go

-- Uygulama Kayıtları (UTC)
select top (100) * from dbo.Logs order by Id desc;

-- Trigger Kayıtları (UTC)
select top (100) * from dbo.Logs_Trigger order by Id desc;
go
