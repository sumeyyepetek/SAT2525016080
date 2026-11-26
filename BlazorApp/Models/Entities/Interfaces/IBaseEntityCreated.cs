namespace Entities;

public interface IBaseEntityCreated
{

    // Özellikler
    string? CreatedBy { get; set; }

    DateTime CreatedDateTime { get; set; }
}
