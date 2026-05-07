using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }
    public UserDataRoot Data { get; private set; }
    public string CurrentUserId { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool IsDirty { get; private set; }
    public bool IsBusy { get; private set; }

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

            if (snapshot.Exists)
            {
                Data = snapshot.ConvertTo<UserDataRoot>();

                if (Data == null)
                {
                    Debug.LogWarning("[UserDataManager] ConvertTo returned null. Creating default data.");
                    Data = UserDataFactory.CreateDefault(userId);

                    if (!await SaveAsync(true))
                        return false;
                }
                else
                {
                    if (Data.Profile == null)
                        Data.Profile = UserDataFactory.CreateDefaultProfile(userId);

                    if (Data.Resources == null)
                        Data.Resources = UserDataFactory.CreateDefaultResources();

                    if (Data.Roster == null)
                        Data.Roster = UserDataFactory.CreateDefaultRoster();

                    if (Data.Progress == null)
                        Data.Progress = UserDataFactory.CreateDefaultProgress();

                    Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
                }
            }
            else
            {
                Data = UserDataFactory.CreateDefault(userId);

                if(!await SaveAsync(true))
                    return false;
                
                Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
            }

            bool fuelChanged = StaminaService.RefreshFuel(Data.Resources);

            if (fuelChanged)
                await SaveAsync(true);

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
        
        if(Data == null)
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
            await docRef.SetAsync(Data);

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


    public void MarkDirty()
    {
        IsDirty = true;
    }
}

