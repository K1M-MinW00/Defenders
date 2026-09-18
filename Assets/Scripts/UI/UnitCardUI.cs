using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitCardUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject promotionInfoRoot;
    [SerializeField] private TMP_Text promotionText;
    [SerializeField] private Image prom_Img;
    [SerializeField] private Sprite[] promotion_sprites;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("State")]
    [SerializeField] private Color ownedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color normalBackgroundColor = Color.blue;
    [SerializeField] private Color rareBackgroundColor = new Color(0.627451f, 0.12549f, 0.941176f, 1f);
    [SerializeField] private Color legendBackgroundColor = new Color(1f, 0.921569f, 0.015686f, 1f);

    [Header("Long Press")]
    [SerializeField] private float longPressSeconds = 0.45f;

    [Header("Shake")]
    [SerializeField] private float shakeAngle = 4f;
    [SerializeField] private float shakeSpeed = 18f;

    private LobbyUnitViewModel viewModel;

    private Coroutine longPressRoutine;
    private Coroutine shakeRoutine;

    private bool isLongPressed;
    private bool pointerDown;

    public event Action<UnitCardUI, LobbyUnitViewModel> OnClicked;
    public event Action<UnitCardUI, LobbyUnitViewModel> OnLongPressed;

    public LobbyUnitViewModel ViewModel => viewModel;

    public void Bind(LobbyUnitViewModel vm)
    {
        if (vm == null)
            return;
     
        viewModel = vm;
        bool isOwned = vm.IsOwned;

        if (backgroundImage != null)
            backgroundImage.color = GetRarityColor(vm.Rarity);

        if (promotionInfoRoot != null)
            promotionInfoRoot.SetActive(isOwned);

        if (promotionText != null)
            promotionText.SetText("{0}", vm.Promotion);

        if (prom_Img != null)
        {
            int promotion = vm.Promotion;
            bool hasPromotionSprite = promotion_sprites != null &&
                promotion >= 0 && promotion < promotion_sprites.Length;

            prom_Img.enabled = isOwned && hasPromotionSprite;

            if (prom_Img.enabled)
                prom_Img.sprite = promotion_sprites[promotion];
        }

        if (iconImage != null)
        {
            iconImage.sprite = vm.Icon;
            iconImage.color = isOwned ? ownedColor : lockedColor;
        }

        if (levelText != null)
        {
            levelText.text = isOwned ? $"Lv.{vm.Level}" : "Locked";
        }

        if (canvasGroup != null)
        {
            canvasGroup.interactable = isOwned;
            canvasGroup.blocksRaycasts = isOwned;
            canvasGroup.alpha = isOwned ? 1f : 0.5f;
        }

        StopShake();
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => normalBackgroundColor,
            Rarity.Rare => rareBackgroundColor,
            Rarity.Legend => legendBackgroundColor,
            _ => Color.white,
        };
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (viewModel == null)
            return;

        pointerDown = true;
        isLongPressed = false;

        StopLongPressRoutine();
        longPressRoutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        StopLongPressRoutine();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerDown = false;
        StopLongPressRoutine();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (viewModel == null)
            return;

        if (isLongPressed)
            return;

        OnClicked?.Invoke(this, viewModel);
    }

    private IEnumerator LongPressRoutine()
    {
        float elapsed = 0f;

        while (pointerDown && elapsed < longPressSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!pointerDown)
            yield break;

        isLongPressed = true;
        OnLongPressed?.Invoke(this, viewModel);
    }

    public void StartShake()
    {
        StopShake();
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    public void StopShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        transform.localRotation = Quaternion.identity;
    }

    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            float z = Mathf.Sin(Time.unscaledTime * shakeSpeed) * shakeAngle;
            transform.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }
    }

    private void StopLongPressRoutine()
    {
        if (longPressRoutine != null)
        {
            StopCoroutine(longPressRoutine);
            longPressRoutine = null;
        }
    }

    private void OnDisable()
    {
        StopLongPressRoutine();
        StopShake();
    }
}
