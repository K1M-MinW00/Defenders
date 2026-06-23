using UnityEngine;

public class UnitRangeIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer rangeRenderer;

    public void Show(float range)
    {
        if (rangeRenderer == null || rangeRenderer.sprite == null)
            return;

        float scale = range * 2f;
        transform.localScale = new Vector3(scale, scale, 1f);
        rangeRenderer.enabled = true;
    }

    public void Hide()
    {
        if (rangeRenderer == null)
            return;

        rangeRenderer.enabled = false;
    }
}