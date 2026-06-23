using UnityEngine;
public enum UnitDropAction
{
    None,
    Reroll,
    Sell
}

public class UnitDropZone : MonoBehaviour
{
    [SerializeField] private UnitDropAction action = UnitDropAction.None;
    public UnitDropAction Action => action;
}