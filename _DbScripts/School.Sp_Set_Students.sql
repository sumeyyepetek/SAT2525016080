use BlazorApp
go

-- Ekleme, güncelleme ve silme yordamını oluştur veya değiştir
create or alter procedure School.Sp_Set_Students
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
            FirstName nvarchar(max) null,
            LastName nvarchar(max) null,
            Gender int null,
            DepartmentId int null
        )

        -- JSON alanlarını geçici tablo değişkenine aktar
        insert into @temp (OperationType, Id, FirstName, LastName, Gender, DepartmentId)
        select OperationType, Id, FirstName, LastName, Gender, DepartmentId
        from openjson(@Values)
        with
        (
            OperationType int '$.OperationType',
            Id int '$.Id',
            FirstName nvarchar(max) '$.FirstName',
            LastName nvarchar(max) '$.LastName',
            Gender int '$.Gender',
            DepartmentId int '$.DepartmentId'
        )

        -- Etkilenen kayıt sayısı için değişken tanımla
        declare @affected int

        -- Yeni kayıtları ekle
        insert into School.Student (FirstName, LastName, Gender, DepartmentId, CreatedDateTime)
        select t.FirstName, t.LastName, t.Gender, t.DepartmentId, getdate()
        from @temp t
        where t.OperationType = 1

        -- Ekleme sonucunu kaydet
        set @affected = @@rowcount
        insert into @result (OperationType, AffectedCount, Message)
        values (1, @affected, N'insert')

        -- Mevcut kayıtları güncelle
        update s
        set s.FirstName = t.FirstName,
            s.LastName = t.LastName,
            s.Gender = t.Gender,
            s.DepartmentId = t.DepartmentId
        from @temp t
            join School.Student s on s.Id = t.Id
        where t.OperationType = 0

        -- Güncelleme sonucunu kaydet
        set @affected = @@rowcount
        insert into @result (OperationType, AffectedCount, Message)
        values (0, @affected, N'update')

        -- Mevcut kayıtları sil
        delete s
        from @temp t
            join School.Student s on s.Id = t.Id
        where t.OperationType = -1

        -- Silme sonucunu kaydet
        set @affected = @@rowcount
        insert into @result (OperationType, AffectedCount, Message)
        values (-1, @affected, N'delete')

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
declare @deptId int
declare @values nvarchar(max)

-- Mevcut ilk bölümün Id'sini al
select top (1) @deptId = Id from School.Department

-- Ekleme işlemini dene
set @values = N'[{"OperationType":1,"Id":0,"FirstName":"Ahmet","LastName":"Yılmaz","Gender":1,"DepartmentId":' + cast(isnull(@deptId,1) as nvarchar) + '},{"OperationType":1,"Id":0,"FirstName":"Ayşe","LastName":"Kaya","Gender":2,"DepartmentId":' + cast(isnull(@deptId,1) as nvarchar) + '}]'

insert into @result_exec
exec School.Sp_Set_Students @values

-- Güncelleme işlemini dene
select top (1) @id = Id
from School.Student
where FirstName = N'Ahmet'

if @id is not null
begin
    set @values = N'[{"OperationType":0,"Id":' + cast(@id as nvarchar) + ',"FirstName":"Mehmet","LastName":"Yılmaz","Gender":1,"DepartmentId":' + cast(isnull(@deptId,1) as nvarchar) + '}]'

    insert into @result_exec
    exec School.Sp_Set_Students @values
end

-- Silme işlemini dene
select top (1) @id = Id
from School.Student
where FirstName = N'Ayşe'
order by Id desc

if @id is not null
begin
    set @values = N'[{"OperationType":-1,"Id":' + cast(@id as nvarchar) + '}]'

    insert into @result_exec
    exec School.Sp_Set_Students @values
end

-- Deneme sonuçlarını görüntüle
select * from @result_exec where AffectedCount > 0 or Message like N'Hata:%'
select * from School.Student order by Id desc