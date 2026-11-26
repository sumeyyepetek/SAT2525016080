using DbModels;

namespace DbProviders;

public interface IDbProvider<T>
{
    Task GetItemsAsync(IDbModel<T> model);
    Task SetItemsAsync(IDbModel<T> model);
}