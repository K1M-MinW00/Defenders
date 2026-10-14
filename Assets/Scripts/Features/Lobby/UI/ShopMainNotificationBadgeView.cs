using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopMainNotificationBadgeView : MonoBehaviour
{
    [SerializeField] private ShopProductGroup shopProductGroup;
    [SerializeField] private GameObject badge;

    private UserDataManager userDataManager;
    private bool isSubscribed;
    private Coroutine refreshRoutine;

    private void Awake()
    {
        if (shopProductGroup == null || badge == null)
        {
            Debug.LogError("[ShopMainNotificationBadgeView] Required references are not assigned.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
        refreshRoutine = StartCoroutine(RefreshPeriodically());
    }

    private void OnDisable()
    {
        if (refreshRoutine != null)
        {
            StopCoroutine(refreshRoutine);
            refreshRoutine = null;
        }

        Unsubscribe();
    }

    private IEnumerator RefreshPeriodically()
    {
        WaitForSecondsRealtime delay = new(1f);
        while (true)
        {
            yield return delay;
            Subscribe();
            Refresh();
        }
    }

    private void Subscribe()
    {
        userDataManager ??= UserDataManager.Instance;
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnShopUpdated += Refresh;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnShopUpdated -= Refresh;
        isSubscribed = false;
    }

    private void Refresh()
    {
        bool visible = userDataManager != null &&
                       (userDataManager.HasShopNotification(ShopTabType.Limited, shopProductGroup) ||
                        userDataManager.HasShopNotification(ShopTabType.Package, shopProductGroup) ||
                        userDataManager.HasShopNotification(ShopTabType.Recharge, shopProductGroup) ||
                        userDataManager.HasShopNotification(ShopTabType.Exchange, shopProductGroup));
        badge.SetActive(visible);
    }
}
