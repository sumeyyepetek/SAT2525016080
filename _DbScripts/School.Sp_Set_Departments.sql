use BlazorApp
go
 
-- Ekleme, güncelleme ve silme yordamını oluştur veya değiştir
create or alter procedure School.Sp_Set_Departments
    -- İşlenecek verileri JSON biçiminde al
    @Values nvarchar(max) = '[]'
as
begin
    -- Etkilenen satır sayısı iletilerini kapat
    set nocount on
 
    -- Çalışma zamanı hatalarında işlemi geri almayı etkinleştir
    set xact_abort on
 
    -- İşlem sonuçları için tablo değişkeni tanımla
    declare @result table
    (
        -- İşlem türünü ekleme için 1, güncelleme için 0 ve silme için -1 olarak kullan
        OperationType int,
        AffectedCount int not null,
        Message nvarchar(max) not null
    )
 
    -- Hata denetimli işlem bloğunu başlat
    begin try
        -- Veri değişikliği işlemini başlat
        begin transaction
 
        -- Gelen verinin JSON dizisi olduğunu doğrula
        if @Values is null or isjson(@Values) <> 1
            or left(ltrim(@Values), 1) <> '['
        begin
            ;throw 50001, N'Values bir JSON dizisi olmalıdır.', 1;
        end
 
        -- Gelen veriler için geçici tablo değişkeni tanımla
        declare @temp table
        (
            OperationType int null,
            Id int null,
            Name nvarchar(max) null
        )
 
        -- JSON alanlarını geçici tablo değişkenine aktar
        insert into @temp (OperationType, Id, Name)
        select OperationType, Id, Name
        from openjson(@Values)
        with
        (
            OperationType int '$.OperationType',
            Id int '$.Id',
            Name nvarchar(max) '$.Name'
        )
 
        -- Etkilenen kayıt sayısı için değişken tanımla
        declare @affected int
 
        -- Yeni kayıtları ekle
        insert into School.Department (Name, CreatedDateTime)
        select t.Name, getdate()
        from @temp t
        where t.OperationType = 1
 
        -- Ekleme sonucunu kaydet
        set @affected = @@rowcount
        insert into @result (OperationType, AffectedCount, Message)
        values (1, @affected, N'insert')
 
        -- Mevcut kayıtları güncelle
        update d
        set d.Name = t.Name
        from @temp t
            join School.Department d on d.Id = t.Id
        where t.OperationType = 0
 
        -- Güncelleme sonucunu kaydet
        set @affected = @@rowcount
        insert into @result (OperationType, AffectedCount, Message)
        values (0, @affected, N'update')
 
        -- Mevcut kayıtları sil
        delete d
        from @temp t
            join School.Department d on d.Id = t.Id
        where t.OperationType = -1
 
        -- Silme sonucunu kaydet
        set @affected = @@rowcount
        insert into @result (OperationType, AffectedCount, Message)
        values (-1, @affected, N'delete')
 
        -- Hata denetimi için örnek sıfıra bölme işlemini kullan
        --select 1/0
 
        -- Veri değişikliklerini onayla
        commit transaction
    end try
    -- Yakalanan hatayı işle
    begin catch
        -- Açık işlem varsa veri değişikliklerini geri al
        if xact_state() <> 0
            rollback transaction
 
        -- Hata durumunda işlem sonuçlarını temizle
        delete from @result
 
        -- Hata bilgilerini işlem sonuçlarına ekle
        insert into @result (OperationType, AffectedCount, Message)
        select op.OperationType, 0, concat(N'Hata: ', error_number(), ' - ', error_message())
        from (values (1), (0), (-1)) op(OperationType)
    end catch
 
    -- İşlem sonuçlarını döndür
    select OperationType, AffectedCount, Message
    from @result
    order by OperationType
 
end
go
 
-- Deneme sonuçları için tablo değişkeni tanımla
declare @result_exec table
(
    OperationType int,
    AffectedCount int,
    Message nvarchar(max)
)
 
declare @id int
declare @values nvarchar(max)
 
 
-- Ekleme işlemini dene
select @values =
(
    select 1 as OperationType, 0 as Id, value as Name
    from string_split(N'AAA,BBB,CCC',',')
    for json path
)
 
insert into @result_exec
exec School.Sp_Set_Departments @values
 
-- Güncelleme işlemini dene
select top (1) @id = Id
from School.Department
where Name = N'CCC'
 
select @values =
(
    select 0 as OperationType, @id as Id, N'XXX' as Name
    for json path
)
 
insert into @result_exec
exec School.Sp_Set_Departments @values
 
-- Silme işlemini dene
select top (1) @id = Id
from School.Department
where Name = N'BBB'
order by Id desc
 
select @values =
(
    select -1 as OperationType, @id as Id
    for json path
)
 
insert into @result_exec
exec School.Sp_Set_Departments @values
 
-- Deneme sonuçlarını görüntüle
select * from @result_exec where AffectedCount > 0 or Message like N'Hata:%'
select * from School.Department order by Id desc
 