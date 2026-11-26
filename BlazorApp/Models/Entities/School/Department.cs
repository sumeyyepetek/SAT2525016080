namespace Entities;

public class Department : BaseEntityCreated, IBaseEntity, IBaseEntityName
{
    // Skaler özellikler
    public int Id { get; set; }
    public string? Name { get; set; }

    // Gezinme özellikleri
    public List<Student> Students { get; set; } = [];


    // Metotlar
    public override string ToString() => $"{Id} - {Name}";
}
