use BlazorApp
go

-- Filtreleme, sıralama ve sayfalama yordamını oluştur veya değiştir
create or alter procedure School.Sp_Get_Students
    -- Sayfalama ve sıralama ölçütlerini JSON biçiminde al
    @Pagination nvarchar(max),
    -- Filtreleme ölçütlerini JSON biçiminde al
    @Where nvarchar(max)
as
begin
    -- Etkilenen satır sayısı iletilerini kapat
    set nocount on

    -- Sayfalama değişkenlerini hazırla
    declare @PageNumber int = try_convert(int, json_value(@Pagination, '$.PageNumber'))
    declare @PageSize int = try_convert(int, json_value(@Pagination, '$.PageSize'))

    -- Sıralama değişkenini hazırla
    declare @OrderBy nvarchar(100) = json_value(@Pagination, '$.OrderBy')

    -- Geçersiz sayfalama değerlerini düzelt
    if @PageNumber is null or @PageNumber < 1
        set @PageNumber = 1

    if @PageSize is null or @PageSize < 1
        set @PageSize = 10

    if @PageSize > 1000
        set @PageSize = 1000

    -- Filtreleme değişkenlerini hazırla
    declare @Id nvarchar(4000) = nullif(replace(trim(json_value(@Where, '$.Id')), ',', ';'), '')
    declare @FirstName nvarchar(4000) = nullif(replace(trim(json_value(@Where, '$.FirstName')), ',', ';'), '')
    declare @LastName nvarchar(4000) = nullif(replace(trim(json_value(@Where, '$.LastName')), ',', ';'), '')
    declare @Gender int = try_convert(int, json_value(@Where, '$.Gender'))
    declare @DepartmentId int = try_convert(int, json_value(@Where, '$.DepartmentId'))

    -- Kimlik filtrelerini alt ve üst sınır aralıklarına ayrıştır
    declare @IdRanges table (MinId int not null, MaxId int not null)

    insert into @IdRanges (MinId, MaxId)
    select limits.MinId, limits.MaxId
    from string_split(@Id, ';') ss
    cross apply (values (charindex('-', ss.value))) dash(Position)
    cross apply (values
    (
        try_convert(int, nullif(trim(case when dash.Position = 0
            then ss.value else left(ss.value, nullif(dash.Position, 0) - 1) end), '')),
        try_convert(int, nullif(trim(case when dash.Position = 0
            then ss.value else substring(ss.value, dash.Position + 1, 4000) end), ''))
    )) limits(MinId, MaxId)
    where limits.MinId is not null and limits.MaxId is not null

    -- Filtreleme, sıralama ve sayfalama sorgusunu başlat
    ;with
    -- Temel kayıtları seç (Department name ile birlikte)
    cte_base as
    (
        select s.Id, s.FirstName, s.LastName, s.Gender, s.DepartmentId, d.Name as DepartmentName, s.CreatedBy, s.CreatedDateTime
        from School.Student s
        left join School.Department d on s.DepartmentId = d.Id
    )
    -- Kayıtları verilen ölçütlere göre filtrele
    ,cte_filter as
    (
        select *
        from cte_base b
        where (@Id is null or exists
              (select 1
               from @IdRanges r
               where b.Id between r.MinId and r.MaxId))
          and (@FirstName is null or exists
              (select 1
               from string_split(@FirstName, ';') ss
               where trim(ss.value) <> ''
                 and charindex(trim(ss.value), b.FirstName, 1) > 0))
          and (@LastName is null or exists
              (select 1
               from string_split(@LastName, ';') ss
               where trim(ss.value) <> ''
                 and charindex(trim(ss.value), b.LastName, 1) > 0))
          and (@Gender is null or b.Gender = @Gender)
          and (@DepartmentId is null or b.DepartmentId = @DepartmentId)
    )
    -- Kayıtları sırala ve satırları numaralandır
    ,cte_order as
    (
        select *, row_number() over (order by
            case when @OrderBy = 'Id ASC' then Id end asc,
            case when @OrderBy = 'Id DESC' then Id end desc,
            case when @OrderBy = 'FirstName ASC' then FirstName end asc,
            case when @OrderBy = 'FirstName DESC' then FirstName end desc,
            case when @OrderBy = 'LastName ASC' then LastName end asc,
            case when @OrderBy = 'LastName DESC' then LastName end desc,
            Id asc
        ) as RowNumber
        from cte_filter
    )
    -- Toplam sayfa sayısını ve geçerli sayfa numarasını hesapla
    ,cte_page_summary as
    (
        select case when @PageNumber > TotalPageCount and TotalPageCount > 0
                    then TotalPageCount else @PageNumber end as PageNumber,
               TotalRecordCount, TotalPageCount
        from
        (
            select TotalRecordCount,
                   convert(int, ceiling(TotalRecordCount * 1.0 / @PageSize)) as TotalPageCount
            from (select count(*) as TotalRecordCount from cte_filter) trc
        ) totals
    )
    -- İstenen sayfadaki kayıtları seç
    ,cte_pagination as
    (
        select o.*, p.PageNumber, p.TotalRecordCount, p.TotalPageCount
        from cte_order o
            cross join cte_page_summary p
        where o.RowNumber > (p.PageNumber - 1) * @PageSize
          and o.RowNumber <= p.PageNumber * @PageSize
    )
    -- Döndürülecek kayıt kümesini hazırla
    ,cte_finally as
    (
        select *, row_number() over (order by RowNumber) as PageRowNumber
        from cte_pagination
    )
    -- Sorgu sonuçlarını, sayfalama bilgileri ilk satırda olacak şekilde döndür
    select Id, FirstName, LastName, Gender, DepartmentId, DepartmentName, CreatedBy, CreatedDateTime, RowNumber
            , iif(PageRowNumber=1, PageNumber, null) PageNumber
            , iif(PageRowNumber=1, TotalRecordCount, null) TotalRecordCount
            , iif(PageRowNumber=1, TotalPageCount, null) TotalPageCount
    from cte_finally
    order by RowNumber
end
go

-- Filtreleme, sıralama ve sayfalama işlemlerini dene
exec School.Sp_Get_Students
    @Pagination = '{"PageNumber":1, "PageSize":100, "OrderBy":"Id ASC"}',
    @Where = '{"FirstName": "a", "Gender": 1}'