using UnityEngine;

public class UnitDragContext : MonoBehaviour
{
    public PlayerCharacter DraggingUnit { get; private set; }
    public Vector3 OriginalPos { get; private set; }

    public void Begin(PlayerCharacter unit)
    {
        DraggingUnit = unit;
        OriginalPos = unit.transform.position;
    }

    public void End()
    {
        DraggingUnit = null;
    }
}