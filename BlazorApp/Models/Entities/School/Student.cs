namespace Entities;

public class Student : BaseEntityCreated, IBaseEntity, IBaseEntityFirstAndLastName
{
    // Skaler özellikler
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public byte Gender { get; set; }
    public int? DepartmentId { get; set; }

    // Gezinme özellikleri
    public Department? Department { get; set; }
}
