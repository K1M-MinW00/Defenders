using System;
using System.Threading.Tasks;
using UnityEngine;

public partial class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }
    public UserDataRoot UserData { get; private set; }
    public InventoryService InventoryService { get; private set; }
    public MailboxService MailboxService { get; private set; }
    public GachaService GachaService { get; private set; }
    public RosterService RosterService { get; private set; }
    public UnitTrainingUseCase UnitTrainingUseCase { get; private set; }
    public UnitPromotionUseCase UnitPromotionUseCase { get; private set; }
    public UnitLimitBreakUseCase UnitLimitBreakUseCase { get; private set; }
    public GachaUseCase GachaUseCase { get; private set; }
    public PurchaseFuelUseCase PurchaseFuelUseCase { get; private set; }
    public ClaimAdFuelRewardUseCase ClaimAdFuelRewardUseCase { get; private set; }
    public UnitFormationUseCase UnitFormationUseCase { get; private set; }
    public ProfileUpdateUseCase ProfileUpdateUseCase { get; private set; }

    public string CurrentUserId { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action OnProfileUpdated;
    public event Action OnResourceUpdated;
    public event Action OnRosterUpdated;
    public event Action OnProgressUpdated;

    private IUserDataRepository repository;
    private UserDataLoader userDataLoader;

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
            userDataLoader = new UserDataLoader(repository);
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
            UserDataRoot loadedUserData = await userDataLoader.LoadOrCreateAsync(userId);

            CurrentUserId = userId;
            UserData = loadedUserData;
            ComposeServices();
            Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");

            IsLoaded = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataManager] LoadOrCreateAsync failed: {e}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ComposeServices()
    {
        InventoryService = new InventoryService(UserData);
        MailboxService = new MailboxService(CurrentUserId, UserData);
        GachaService = new GachaService(UserData);
        RosterService = new RosterService(UserData);
        UnitTrainingUseCase = new UnitTrainingUseCase(repository, CurrentUserId, UserData);
        UnitPromotionUseCase = new UnitPromotionUseCase(repository, CurrentUserId, UserData);
        UnitLimitBreakUseCase = new UnitLimitBreakUseCase(repository, CurrentUserId, UserData);
        GachaUseCase = new GachaUseCase(repository, CurrentUserId, UserData, new UnityGachaRandom());
        PurchaseFuelUseCase = new PurchaseFuelUseCase(repository, CurrentUserId, UserData);
        ClaimAdFuelRewardUseCase = new ClaimAdFuelRewardUseCase(repository, CurrentUserId, UserData);
        UnitFormationUseCase = new UnitFormationUseCase(repository, CurrentUserId, UserData);
        ProfileUpdateUseCase = new ProfileUpdateUseCase(repository, CurrentUserId, UserData);
    }

    private Task<bool> SaveProgressAsync(UserProgressData progress) =>
        SaveSectionAsync(progress, () => repository.SaveProgressAsync(CurrentUserId, progress), "progress");

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

    public void RaiseProfileUpdated()
    {
        OnProfileUpdated?.Invoke();
    }

    public void RaiseResourceUpdated()
    {
        OnResourceUpdated?.Invoke();
    }

    public void RaiseRosterUpdated()
    {
        OnRosterUpdated?.Invoke();
    }

    public void RaiseProgressUpdated()
    {
        OnProgressUpdated?.Invoke();
    }
}
