using System.Collections.Generic;
using UnityEngine;

public interface IUnitCatalog
{
    UnitDataSO Get(string unitId);
    Sprite GetIcon(string unitId);
    IReadOnlyCollection<UnitDataSO> GetAll();
}
