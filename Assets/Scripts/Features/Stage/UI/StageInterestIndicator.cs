using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageInterestIndicator : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite filledSprite;

    [Header("Slots")]
    [SerializeField] private Image[] slots = new Image[5];

    public IReadOnlyList<Image> Slots => slots;

    public void SetInterest(int interest)
    {
        if (slots == null)
            return;

        int filledCount = Mathf.Clamp(interest, 0, slots.Length);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            slots[i].sprite = i < filledCount ? filledSprite : emptySprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetInterest(0);
    }
#endif
}
