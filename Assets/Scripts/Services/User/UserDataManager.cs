using System;
using System.Threading.Tasks;
using UnityEngine;

public partial class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }
    public UserDataRoot UserData { get; private set; }
    public InventoryService InventoryService { get; private set; }
    public MailboxService MailboxService { get; private set; }
    public ResourceService ResourceService { get; private set; }
    public GachaService GachaService { get; private set; }
    public RosterService RosterService { get; private set; }
    public RewardService RewardService { get; private set; }
    public UnitTrainingUseCase UnitTrainingUseCase { get; private set; }
    public UnitPromotionUseCase UnitPromotionUseCase { get; private set; }

    public string CurrentUserId { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool IsDirty { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action OnProfileUpdated;
    public event Action OnResourceUpdated;
    public event Action OnProgressUpdated;

    private IUserDataRepository repository;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Initialize(IUserDataRepository dataRepository = null)
    {
        if (IsInitialized)
            return true;

        try
        {
            repository = dataRepository ?? new FirestoreUserDataRepository();
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataManager] Repository initialization failed: {e}");
            return false;
        }

        IsInitialized = true;
        Debug.Log("[UserDataManager] Firestore initialized.");
        return true;
    }

    public async Task<bool> LoadOrCreateAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[UserDataManager] userId is Null or Empty");
            return false;
        }

        if (IsBusy)
        {
            Debug.LogWarning("[UserDataManager] Another task is already running");
            return false;
        }

        if (!Initialize())
            return false;

        IsBusy = true;
        IsLoaded = false;

        try
        {
            CurrentUserId = userId;

            UserDataLoadResult loadResult = await repository.LoadAsync(userId);

            bool isNewUser = false;
            bool needsSave = false;

            if (loadResult.Exists)
            {
                UserData = loadResult.Data;

                if (UserData == null)
                {
                    UserData = UserDataFactory.CreateDefault(userId);
                    isNewUser = true;
                    needsSave = true;
                }
                else
                {
                    needsSave = UserDataMigrator.MigrateToCurrent(UserData, userId);
                    Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
                }
            }
            else
            {
                UserData = UserDataFactory.CreateDefault(userId);
                isNewUser = true;
                needsSave = true;

                Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
            }

            if (isNewUser)
            {
                StaminaService.InitializeFullFuel(UserData.Resource);
            }
            else
            {
                bool fuelChanged = StaminaService.RefreshFuel(UserData.Resource);
                needsSave |= fuelChanged;
            }

            if (needsSave)
            {
                bool saveSucceeded = isNewUser
                    ? await CreateUserAsync()
                    : await SaveAsync(true);

                if (!saveSucceeded)
                    return false;
            }

            InventoryService = new InventoryService();
            MailboxService = new MailboxService();
            ResourceService = new ResourceService();
            RewardService = new RewardService();
            GachaService = new GachaService();
            RosterService = new RosterService();
            UnitTrainingUseCase = new UnitTrainingUseCase(repository, CurrentUserId, UserData);
            UnitPromotionUseCase = new UnitPromotionUseCase(repository, CurrentUserId, UserData);

            IsLoaded = true;
            IsDirty = false;

            return true;
        }
        catch (Exception e)
        {
            Debug.Log($"[UserDataManager] LoadOrCreate Async exception : {e}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> SaveAsync(bool force = false)
    {
        if (!IsInitialized || repository == null)
        {
            Debug.LogError("[UserDataManager] Not Initialized");
            return false;
        }

        if (UserData == null)
        {
            Debug.LogError("[UserDataManager] No loaded data to save.");
            return false;
        }

        if (!force && !IsDirty)
            return true;

        try
        {
            await repository.SaveAllAsync(CurrentUserId, UserData);

            IsDirty = false;

            Debug.Log($"[UserDataManager] Save success. UID : {CurrentUserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataManager] SaveAsync exception : {e}");
            return false;
        }
    }

    public Task<bool> SaveProfileAsync(UserProfileData profile) =>
        SaveSectionAsync(profile, () => repository.SaveProfileAsync(CurrentUserId, profile), "profile");

    public Task<bool> SaveResourcesAsync(UserResourceData resources) =>
        SaveSectionAsync(resources, () => repository.SaveResourcesAsync(CurrentUserId, resources), "resources");

    public Task<bool> SaveProgressAsync(UserProgressData progress) =>
        SaveSectionAsync(progress, () => repository.SaveProgressAsync(CurrentUserId, progress), "progress");

    public Task<bool> SaveRosterAsync(UserRosterData roster) =>
        SaveSectionAsync(roster, () => repository.SaveRosterAsync(CurrentUserId, roster), "roster");

    public Task<bool> SaveInventoryAsync(UserInventoryData inventory) =>
        SaveSectionAsync(inventory, () => repository.SaveInventoryAsync(CurrentUserId, inventory), "inventory");

    public Task<bool> SaveGachaAsync(UserGachaData gacha) =>
        SaveSectionAsync(gacha, () => repository.SaveGachaAsync(CurrentUserId, gacha), "gacha");

    public Task<bool> SaveAdAsync(UserAdData ad) =>
        SaveSectionAsync(ad, () => repository.SaveAdAsync(CurrentUserId, ad), "ad");

    public async Task<bool> SaveUserProgressAsync(UserProgressData progress)
    {
        bool success = await SaveProgressAsync(progress);

        if (success && UserData != null)
        {
            UserData.Progress = progress;
            RaiseProgressUpdated();
        }

        return success;
    }

    private async Task<bool> CreateUserAsync()
    {
        try
        {
            await repository.CreateAsync(CurrentUserId, UserData);
            IsDirty = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataManager] Create user failed: {e}");
            return false;
        }
    }

    private async Task<bool> SaveSectionAsync<T>(T value, Func<Task> saveOperation, string sectionName)
        where T : class
    {
        if (!IsInitialized || repository == null || string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.LogError($"[UserDataManager] Cannot save {sectionName}. Manager is not ready.");
            return false;
        }

        if (value == null)
        {
            Debug.LogError($"[UserDataManager] Cannot save {sectionName}. Data is null.");
            return false;
        }

        try
        {
            await saveOperation();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataManager] Save {sectionName} failed: {e}");
            return false;
        }
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void RaiseProfileUpdated()
    {
        OnProfileUpdated?.Invoke();
    }

    public void RaiseResourceUpdated()
    {
        OnResourceUpdated?.Invoke();
    }

    public void RaiseProgressUpdated()
    {
        OnProgressUpdated?.Invoke();
    }
    private async void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            await SaveAsync();
        }
    }

    private async void OnApplicationQuit()
    {
        await SaveAsync();
    }
}
