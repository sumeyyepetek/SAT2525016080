using DbModels;
using Entities;
using Enums;
 
namespace Helpers;
 
public sealed class XFormHelper<TItem>(IDbModel<TItem> model)
    where TItem : class, IBaseEntity, new()
{
    public DbItem<TItem>? Row { get; private set; }
 
    public void Open(Operation operation, TItem? item = null)
    {
        if (model.IsBusy) return;
        if (operation != Operation.Insert &&
            !model.Items.Any(row => ReferenceEquals(row.Item, item))) return;
 
        Row = new DbItem<TItem>(item ?? new TItem(), operation == Operation.Insert)
        {
            Operation = operation
        };
    }
 
    public void Save(TItem entity)
    {
        if (model.IsBusy || Row?.Operation is null) return;
 
        if (Row.Operation == Operation.Insert)
        {
            Row.Item = entity;
            model.Items.Insert(0, Row);
        }
        else
        {
            var row = model.Items.FirstOrDefault(row => row.Item.Id == Row.Item.Id);
            if (row is not null)
            {
                row.Item = entity;
                row.Operation = Row.Operation;
                row.Selected = true;
            }
        }
 
        Close();
    }
 
    public void Close() => Row = null;
}
 