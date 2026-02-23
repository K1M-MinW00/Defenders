using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDropRouter : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;

    private readonly List<RaycastResult> results = new();

    private void Awake()
    {
        if (eventSystem == null) eventSystem = EventSystem.current;
        if (raycaster == null) raycaster = FindFirstObjectByType<GraphicRaycaster>();
    }

    public bool TryGetDropAction(Vector2 screenPos, out UnitDropAction action)
    {
        action = UnitDropAction.None;

        if (raycaster == null || eventSystem == null)
            return false;

        results.Clear();
        var ped = new PointerEventData(eventSystem) { position = screenPos };
        raycaster.Raycast(ped, results);

        for (int i = 0; i < results.Count; i++)
        {
            var zone = results[i].gameObject.GetComponentInParent<UnitDropZone>();
            if (zone == null) continue;

            action = zone.Action;
            return action != UnitDropAction.None;
        }

        return false;
    }
}
