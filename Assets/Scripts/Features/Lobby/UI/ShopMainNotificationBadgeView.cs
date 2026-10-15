using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopMainNotificationBadgeView : MonoBehaviour
{
    private const float RefreshIntervalSeconds = 30f;

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
        if (TryInitialize())
            Subscribe();
        Refresh();
        refreshRoutine = StartCoroutine(RefreshPeriodically());
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            Unsubscribe();
            userDataManager = manager;
        }

        if (!isActiveAndEnabled || !TryInitialize())
            return;

        Subscribe();
        Refresh();
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
        WaitForSecondsRealtime delay = new(RefreshIntervalSeconds);
        while (true)
        {
            yield return delay;
            if (TryInitialize())
                Subscribe();
            Refresh();
        }
    }

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        return userDataManager?.UserData != null;
    }

    private void Subscribe()
    {
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

    private void OnDestroy()
    {
        Unsubscribe();
        if (refreshRoutine != null)
            StopCoroutine(refreshRoutine);
    }
}
