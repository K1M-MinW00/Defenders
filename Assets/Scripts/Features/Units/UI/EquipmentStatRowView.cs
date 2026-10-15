using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EquipmentStatRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    public void SetText(string value)
    {
        if (label == null)
        {
            Debug.LogError("[EquipmentStatRowView] Label is not assigned.", this);
            return;
        }

        label.gameObject.SetActive(true);
        label.enabled = true;
        label.text = value ?? string.Empty;
    }
}
