using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EquipmentStatRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    public void SetText(string value)
    {
        label.text = value ?? string.Empty;
    }
}
