using System;
using TMPro;
using UnityEngine;

public class FuelPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private TMP_Text nextRecoverText;
    [SerializeField] private TMP_Text fullRecoverText;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            timer = 0f;
            Refresh();
        }
    }

    private void Refresh()
    {
        var resources = UserDataManager.Instance.UserData.Resource;

        StaminaService.RefreshFuel(resources);

        fuelText.text = $"{resources.Fuel}/{resources.MaxFuel}";

        int nextSeconds = StaminaService.GetRemainingSecondsToNextFuel(resources);

        nextRecoverText.text = $"다음 충전까지 {Format(nextSeconds)}";

        int remainFuel = resources.MaxFuel - resources.Fuel;

        int fullSeconds = StaminaService.GetRemainingSecondsToFullFuel(resources);

        fullRecoverText.text = $"최대 충전까지 {Format(fullSeconds)}";
    }

    private string Format(int seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);

        return $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }
}