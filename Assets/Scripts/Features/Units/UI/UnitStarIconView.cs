using UnityEngine;
using UnityEngine.UI;

public sealed class UnitStarIconView : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] starSprites = new Sprite[4];

    public void SetStar(int star)
    {
        if (targetImage == null || starSprites == null || starSprites.Length == 0)
            return;

        int index = Mathf.Clamp(star, 1, starSprites.Length) - 1;
        Sprite sprite = starSprites[index];
        targetImage.sprite = sprite;
        targetImage.enabled = sprite != null;
    }
}
