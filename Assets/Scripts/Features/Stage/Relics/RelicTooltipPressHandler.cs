using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class RelicTooltipPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Action onPressed;
    private Action onReleased;

    public void Initialize(Action pressed, Action released)
    {
        onPressed = pressed;
        onReleased = released;
    }

    public void OnPointerDown(PointerEventData eventData) => onPressed?.Invoke();

    public void OnPointerUp(PointerEventData eventData) => onReleased?.Invoke();

    private void OnDisable() => onReleased?.Invoke();
}
