using System.Collections.Generic;

public interface IStageCatalog
{
    StageDataSO Get(int sector, int stage);
    IReadOnlyCollection<StageDataSO> GetAll();
}
