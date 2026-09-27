using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Threading.Tasks;

public sealed class LobbyShopPanelView : MonoBehaviour
{
    [System.Serializable]
    private sealed class ShopTabEntry
    {
        public ShopTabType Type;
        public Button Button;
        public GameObject Selected;
        public GameObject Badge;
        public GameObject Panel;
        public Transform Content;
    }

    [Header("Data")]
    [SerializeField] private ShopProductGroup shopProductGroup;

    [Header("Product prefabs")]
    [SerializeField] private GameObject limitedItemPrefab;
    [SerializeField] private GameObject packageItemPrefab;
    [SerializeField] private GameObject rechargeItemPrefab;
    [SerializeField] private GameObject exchangeItemPrefab;

    [Header("Package sections")]
    [SerializeField] private Transform dailyPackageContent;
    [SerializeField] private Transform weeklyPackageContent;
    [SerializeField] private Transform monthlyPackageContent;

    [Header("Tabs")]
    [SerializeField] private ShopTabEntry[] tabs;
    [SerializeField] private ShopTabType defaultTab = ShopTabType.Limited;

    [Header("Header and feedback")]
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private ShopPurchaseConfirmPopup purchaseConfirmPopup;
    [SerializeField] private ShopPurchaseResultPopup purchaseResultPopup;

    private readonly List<ShopProductView> itemViews = new();
    private UnityAction[] tabHandlers;
    private UserDataManager subscribedManager;
    private Coroutine timerRoutine;
    private Coroutine gemAnimationRoutine;
    private bool itemsCreated;

    private void Awake()
    {
        BindTabs();
        EnsureItemsCreated();
    }

    private void OnEnable()
    {
        EnsureItemsCreated();
        Subscribe();
        RefreshBadges();
        ShowTab(defaultTab);
        RefreshGemBalance();
        RefreshItemStates();
        timerRoutine = StartCoroutine(RefreshTimers());
    }

    private void OnDisable()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        if (gemAnimationRoutine != null)
        {
            StopCoroutine(gemAnimationRoutine);
            gemAnimationRoutine = null;
        }

        Unsubscribe();
    }

    public void ShowTab(ShopTabType tab)
    {
        if (tabs == null)
            return;

        foreach (ShopTabEntry entry in tabs)
        {
            if (entry == null)
                continue;

            bool selected = entry.Type == tab;
            if (entry.Panel != null)
                entry.Panel.SetActive(selected);
            if (entry.Selected != null)
                entry.Selected.SetActive(selected);
        }

        _ = MarkTabSeenAndRefreshAsync(tab);
    }

    private void EnsureItemsCreated()
    {
        if (itemsCreated || shopProductGroup == null || tabs == null)
            return;

        CreateItems(shopProductGroup.LimitedProducts, GetTabContent(ShopTabType.Limited), limitedItemPrefab);
        CreateItems(shopProductGroup.DailyPackages, dailyPackageContent, packageItemPrefab);
        CreateItems(shopProductGroup.WeeklyPackages, weeklyPackageContent, packageItemPrefab);
        CreateItems(shopProductGroup.MonthlyPackages, monthlyPackageContent, packageItemPrefab);
        CreateItems(shopProductGroup.RechargeProducts, GetTabContent(ShopTabType.Recharge), rechargeItemPrefab);
        CreateItems(shopProductGroup.ExchangeProducts, GetTabContent(ShopTabType.Exchange), exchangeItemPrefab);
        itemsCreated = true;
    }

    private Transform GetTabContent(ShopTabType tabType)
    {
        return tabs.FirstOrDefault(tab => tab != null && tab.Type == tabType)?.Content;
    }

    private void CreateItems(IEnumerable<ShopProductData> products, Transform parent, GameObject prefab)
    {
        if (parent == null || prefab == null || products == null)
            return;

        foreach (ShopProductData product in products
                     .Where(product => product != null && product.IsEnabled)
                     .OrderBy(product => product.SortOrder))
        {
            ShopProductView item = Instantiate(prefab, parent).GetComponent<ShopProductView>();
            if (item == null)
                continue;

            item.Initialize(product, HandlePurchaseRequested);
            itemViews.Add(item);
        }
    }

    private void HandlePurchaseRequested(ShopProductView item)
    {
        if (item == null || item.ProductData == null || purchaseConfirmPopup == null)
            return;

        ShopProductData product = item.ProductData;
        if (!product.IsGemProduct)
            return;

        int currentGem = GetCurrentGem();
        purchaseConfirmPopup.Show(
            product,
            currentGem,
            () => ConfirmPurchase(product),
            () => ShowTab(ShopTabType.Recharge));
    }

    private async void ConfirmPurchase(ShopProductData product)
    {
        UserDataManager manager = UserDataManager.Instance;
        if (manager == null || product == null)
            return;

        int gemBeforePurchase = GetCurrentGem();

        if (purchaseConfirmPopup != null)
            purchaseConfirmPopup.SetBusy(true);

        ShopPurchaseResult result =
            await manager.PurchaseShopProductAsync(product);

        if (purchaseConfirmPopup != null)
            purchaseConfirmPopup.Hide();

        if (feedbackText != null)
            feedbackText.text = ShopFeedbackMessages.Get(result);

        int gemAfterPurchase = GetCurrentGem();
        if (result != null && result.Succeeded)
            AnimateGemBalance(gemBeforePurchase, gemAfterPurchase);
        else
            RefreshGemBalance();

        RefreshItemStates();

        if (purchaseResultPopup != null)
            purchaseResultPopup.Show(product, result);
    }

    private int GetCurrentGem()
    {
        UserDataManager manager = subscribedManager != null
            ? subscribedManager
            : UserDataManager.Instance;
        return manager != null && manager.UserData != null && manager.UserData.Resource != null
            ? manager.UserData.Resource.Gem
            : 0;
    }
    private void RefreshGemBalance()
    {
        if (gemText != null)
            gemText.text = (subscribedManager?.UserData?.Resource?.Gem ??
                            UserDataManager.Instance?.UserData?.Resource?.Gem ?? 0).ToString("N0");
    }

    private void AnimateGemBalance(int from, int to)
    {
        if (gemAnimationRoutine != null)
            StopCoroutine(gemAnimationRoutine);

        gemAnimationRoutine = StartCoroutine(AnimateGemBalanceRoutine(from, to));
    }

    private IEnumerator AnimateGemBalanceRoutine(int from, int to)
    {
        const float duration = 0.45f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            int value = Mathf.RoundToInt(Mathf.Lerp(from, to, progress));
            if (gemText != null)
                gemText.text = value.ToString("N0");
            yield return null;
        }

        if (gemText != null)
            gemText.text = to.ToString("N0");
        gemAnimationRoutine = null;
    }

    private void RefreshItemStates()
    {
        foreach (ShopProductView item in itemViews)
        {
            if (item != null)
                item.RefreshState();
        }
    }

    private void RefreshBadges()
    {
        UserDataManager manager = subscribedManager != null
            ? subscribedManager
            : UserDataManager.Instance;
        if (tabs != null)
        {
            foreach (ShopTabEntry entry in tabs)
            {
                if (entry == null)
                    continue;

                bool visible = manager != null &&
                               manager.HasShopNotification(entry.Type, shopProductGroup);
                if (entry.Badge != null)
                    entry.Badge.SetActive(visible);
            }
        }
    }

    private async Task MarkTabSeenAndRefreshAsync(ShopTabType tab)
    {
        UserDataManager manager = subscribedManager != null
            ? subscribedManager
            : UserDataManager.Instance;
        if (manager == null || shopProductGroup == null)
            return;

        await manager.MarkShopTabSeenAsync(tab, shopProductGroup);
        if (this != null && isActiveAndEnabled)
            RefreshBadges();
    }

    private IEnumerator RefreshTimers()
    {
        WaitForSecondsRealtime delay = new(1f);
        while (true)
        {
            yield return delay;
            if (subscribedManager == null)
            {
                Subscribe();
                RefreshGemBalance();
                RefreshItemStates();
            }

            foreach (ShopProductView item in itemViews)
            {
                if (item != null && item.gameObject.activeInHierarchy)
                    item.RefreshTimer();
            }
        }
    }

    private void BindTabs()
    {
        int count = tabs?.Length ?? 0;
        tabHandlers = new UnityAction[count];

        for (int i = 0; i < count; i++)
        {
            ShopTabEntry entry = tabs[i];
            if (entry?.Button == null)
                continue;

            ShopTabType type = entry.Type;
            tabHandlers[i] = () => ShowTab(type);
            entry.Button.onClick.AddListener(tabHandlers[i]);
        }
    }

    private void Subscribe()
    {
        UserDataManager manager = UserDataManager.Instance;
        if (manager == null || manager == subscribedManager)
            return;

        Unsubscribe();
        subscribedManager = manager;
        subscribedManager.OnResourceUpdated += HandleResourceUpdated;
        subscribedManager.OnShopUpdated += HandleShopUpdated;
    }

    private void Unsubscribe()
    {
        if (subscribedManager == null)
            return;

        subscribedManager.OnResourceUpdated -= HandleResourceUpdated;
        subscribedManager.OnShopUpdated -= HandleShopUpdated;
        subscribedManager = null;
    }

    private void HandleResourceUpdated()
    {
        RefreshGemBalance();
        RefreshItemStates();
    }

    private void HandleShopUpdated()
    {
        RefreshItemStates();
        RefreshBadges();
    }

    private void OnDestroy()
    {
        Unsubscribe();

        if (tabs == null || tabHandlers == null)
            return;

        for (int i = 0; i < Mathf.Min(tabs.Length, tabHandlers.Length); i++)
        {
            if (tabs[i]?.Button != null && tabHandlers[i] != null)
                tabs[i].Button.onClick.RemoveListener(tabHandlers[i]);
        }
    }
}
