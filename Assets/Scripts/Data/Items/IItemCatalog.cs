using System.Collections.Generic;

public interface IItemCatalog
{
    ItemDataSO Get(string itemId);
    IReadOnlyCollection<ItemDataSO> GetAll();
}
