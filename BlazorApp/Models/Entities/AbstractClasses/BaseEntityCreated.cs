namespace Entities;

public abstract class BaseEntityCreated : IBaseEntityCreated
{

    // Özellikler
    public virtual string? CreatedBy { get; set; } = "aaa";
    public virtual DateTime CreatedDateTime { get; set; } = DateTime.Now;
}
