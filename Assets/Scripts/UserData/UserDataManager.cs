using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }
    public UserDataRoot UserData { get; private set; }
    public InventoryService InventoryService { get;private set; }
    public MailboxService MailboxService { get; private set; }
    public string CurrentUserId { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool IsDirty { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action OnProfileUpdated;
    public event Action OnResourceUpdated;
    public event Action OnProgressUpdated;

    private FirebaseFirestore db;
    private const string UsersCollection = "users";

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

    public bool Initialize()
    {
        if (IsInitialized)
            return true;

        db = FirebaseFirestore.DefaultInstance;
        ItemDatabase.Initialize();
        UnitDatabase.Initialize();
        GameIconDatabase.Initialize();
        if (db == null)
        {
            Debug.LogError("[UserDataManager] FirebaseFirestore.DefaultInstance is null.");
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

            DocumentReference docRef = db.Collection(UsersCollection).Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            bool isNewUser = false;

            if (snapshot.Exists)
            {
                UserData = snapshot.ConvertTo<UserDataRoot>();

                if (UserData == null)
                {
                    UserData = UserDataFactory.CreateDefault(userId);
                    isNewUser = true;
                }
                else
                {
                    if (UserData.Profile == null)
                        UserData.Profile = UserDataFactory.CreateDefaultProfile(userId);

                    if (UserData.Resource == null)
                        UserData.Resource = UserDataFactory.CreateDefaultResources();

                    if (UserData.Roster == null)
                        UserData.Roster = UserDataFactory.CreateDefaultRoster();

                    if (UserData.Progress == null)
                        UserData.Progress = UserDataFactory.CreateDefaultProgress();

                    if(UserData.Inventory == null)
                        UserData.Inventory = UserDataFactory.CreateDefaultInventory();

                    Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
                }
            }
            else
            {
                UserData = UserDataFactory.CreateDefault(userId);
                isNewUser = true;
                
                Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
            }

            if (isNewUser)
            {
                StaminaService.InitializeFullFuel(UserData.Resource);

                await SaveAsync(true);
            }
            else
            {

                Debug.Log($"fuel : {UserData.Resource.Fuel}");
                bool fuelChanged = StaminaService.RefreshFuel(UserData.Resource);

                Debug.Log($"fuel : {UserData.Resource.Fuel}");
                if (fuelChanged)
                {
                    await SaveAsync(true);
                }
            }

            InventoryService = new InventoryService();
            MailboxService = new MailboxService();

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
        if(!IsInitialized || db == null)
        {
            Debug.LogError("[UserDataManager] Not Initialized");
            return false;
        }
        
        if(UserData == null)
        {
            Debug.LogError("[UserDataManager] No Loaded data to save.");
            return false;
        }

        if(string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.LogError("[UserDataManager] CurrentuserId is null or empty");
            return false;
        }

        if (!force && !IsDirty)
            return true;

        try
        {
            DocumentReference docRef = db.Collection(UsersCollection).Document(CurrentUserId);
            await docRef.SetAsync(UserData);

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

    public async Task SaveUserProgressAsync(UserProgressData progress)
    {
        if (progress == null)
        {
            Debug.LogError("SaveUserProgressAsync failed. Progress is null.");
            return;
        }

        if (string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.LogError("SaveUserProgressAsync failed. UserId is null.");
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(CurrentUserId);

        Dictionary<string, object> updates = new()
        {
            { "Progress.CurrentSector", progress.CurrentSector },
            { "Progress.CurrentStage", progress.CurrentStage }
        };

        await userRef.UpdateAsync(updates);

        if (UserData != null)
            UserData.Progress = progress;
    }

    public async Task SaveUserDataAsync()
    {
        if (UserData == null)
        {
            Debug.LogWarning("Save failed: UserData is null.");
            return;
        }

        if (string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.LogError("SaveUserDataAsync failed. UserId is null.");
            return;
        }

        try
        {
            DocumentReference userRef = db.Collection("users").Document(CurrentUserId);

            await userRef.SetAsync(UserData);

            Debug.Log("UserData saved.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveUserDataAsync failed: {ex}");
        }
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void NotifyProfileUpdated()
    {
        OnProfileUpdated?.Invoke();
    }

    public void NotifyResourceUpdated()
    {
        OnResourceUpdated?.Invoke();
    }

    public void NotifyProgressUpdated()
    {
        OnProgressUpdated?.Invoke();
    }
}

