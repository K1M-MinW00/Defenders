using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitResultPopupView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button closeButton;

    [Header("Result")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public void Open(List<GachaResult> results)
    {
        gameObject.SetActive(true);

        Clear();

        foreach (GachaResult result in results)
        {
            RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);

            slot.Setup(result);
        }
    }

    private void Clear()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}