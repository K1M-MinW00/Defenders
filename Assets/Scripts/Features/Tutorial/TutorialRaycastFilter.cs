using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform allowedTarget;

    public void SetAllowedTarget(RectTransform target)
    {
        allowedTarget = target;
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return allowedTarget == null ||
               !RectTransformUtility.RectangleContainsScreenPoint(allowedTarget, screenPoint, eventCamera);
    }
}
