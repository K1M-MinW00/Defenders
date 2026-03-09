using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }
    public UserDataRoot CurrentData { get; private set; }
    public string CurrentUserId { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool IsDirty { get; private set; }
    public bool IsBusy { get; private set; }

    private FirebaseFirestore firestore;
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

    public void Initialize()
    {
        if (IsInitialized)
            return;

        firestore = FirebaseFirestore.DefaultInstance;
        IsInitialized = true;

        Debug.Log("[UserDataManager] Firestore initialized");
    }

    public async Task<bool> LoadOrCreateAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[UserDataManager] userId is Null or Empty");
            return false;
        }

        if (!IsInitialized)
            Initialize();

        if (IsBusy)
        {
            Debug.LogWarning("[UserDataManager] Another task is already running");
            return false;
        }

        IsBusy = true;

        try
        {
            CurrentUserId = userId;

            DocumentReference docRef = firestore.Collection(UsersCollection).Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                CurrentData = snapshot.ConvertTo<UserDataRoot>();

                if (CurrentData == null)
                {
                    Debug.LogWarning("[UserDataManager] ConvertTo returned null. Creating default data.");
                    CurrentData = UserDataFactory.CreateDefault(userId);
                    
                    bool saveOk = await SaveAsync(true);
                    if (!saveOk)
                        return false;
                }
                else
                {
                    if (CurrentData.Profile == null)
                        CurrentData.Profile = new UserProfileData();

                    if (CurrentData.Roster == null)
                        CurrentData.Roster = new UserRosterData();

                    if (CurrentData.Progress == null)
                        CurrentData.Progress = new UserProgressData();

                    if (string.IsNullOrEmpty(CurrentData.Profile.UserId))
                        CurrentData.Profile.UserId = userId;

                    Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
                }
            }
            else
            {
                CurrentData = UserDataFactory.CreateDefault(userId);

                bool saveOk = await SaveAsync(true);

                if(!saveOk)
                    return false;
                
                Debug.Log($"[UserDataManager] User data loaded. UID : {userId}");
            }

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
        if(!IsInitialized)
        {
            Debug.LogError("[UserDataManager] Not Initialized");
            return false;
        }
        
        if(CurrentData == null)
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
            DocumentReference docRef = firestore.Collection(UsersCollection).Document(CurrentUserId);
            await docRef.SetAsync(CurrentData);

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

}

