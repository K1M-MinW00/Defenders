using System.Collections;
using UnityEngine;

public sealed class ShopMainNotificationBadgeView : MonoBehaviour
{
    [SerializeField] private ShopProductGroup shopProductGroup;
    [SerializeField] private GameObject badge;

    private UserDataManager subscribedManager;
    private Coroutine refreshRoutine;

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
        UserDataManager manager = UserDataManager.Instance;
        if (manager == null || manager == subscribedManager)
            return;

        Unsubscribe();
        subscribedManager = manager;
        subscribedManager.OnShopUpdated += Refresh;
    }

    private void Unsubscribe()
    {
        if (subscribedManager == null)
            return;

        subscribedManager.OnShopUpdated -= Refresh;
        subscribedManager = null;
    }

    private void Refresh()
    {
        if (badge == null)
            return;

        UserDataManager manager = subscribedManager != null
            ? subscribedManager
            : UserDataManager.Instance;
        bool visible = manager != null && shopProductGroup != null &&
                       (manager.HasShopNotification(ShopTabType.Limited, shopProductGroup) ||
                        manager.HasShopNotification(ShopTabType.Package, shopProductGroup) ||
                        manager.HasShopNotification(ShopTabType.Recharge, shopProductGroup) ||
                        manager.HasShopNotification(ShopTabType.Exchange, shopProductGroup));
        badge.SetActive(visible);
    }
}
