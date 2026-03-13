using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text promotionText;

    public void Bind(LobbyUnitViewModel vm)
    {
        if (vm == null)
            return;

        if (iconImage != null)
            iconImage.sprite = vm.Icon;
    }
}
