using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser CurrentUser => Auth?.CurrentUser;

    public bool IsInitialized { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action<FirebaseUser> OnLoginSucceeded;
    public event Action<string> OnLoginFailed;

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


    public async Task InitializeAndLoginAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await InitializeFirebaseAsync();

            if (!IsInitialized)
            {
                RaiseLoginFailed("Firebase 초기화 실패");
                return;
            }

            if (CurrentUser != null)
            {
                bool isSessionValid = await ValidateCurrentUserSessionAsync();

                if (isSessionValid)
                {
                    Debug.Log($"[AuthManager] Auto login Success. UID : {CurrentUser.UserId}");
                    OnLoginSucceeded?.Invoke(CurrentUser);
                    return;
                }

                Debug.LogWarning("[AuthManager] Cached User session is invalid. Signing out");
                SignOut();
            }

            var authResult = await Auth.SignInAnonymouslyAsync();
            Debug.Log($"[AuthManager] Anonymous login success. UID : {CurrentUser.UserId}");
            OnLoginSucceeded?.Invoke(CurrentUser);
        }
        catch (Exception e)
        {
            RaiseLoginFailed($"로그인 처리 중 예외 발생 : {e}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task InitializeFirebaseAsync()
    {
        if (IsInitialized)
            return;

        try
        {
            var dependecyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependecyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"[AuthManager] Firebase Dependency error : {dependecyStatus}");
                return;
            }

            Auth = FirebaseAuth.DefaultInstance;
            IsInitialized = true;

            Debug.Log("[AuthManager] Firebase Auth Initiailzed");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AuthManager] Firebase initialize exception : {e}");
        }
    }

    private async Task<bool> ValidateCurrentUserSessionAsync()
    {
        try
        {
            await CurrentUser.TokenAsync(true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AuthManager] Session validation failed : {e}");
            return false;
        }
    }

    public void SignOut()
    {
        if (!IsInitialized || Auth == null)
            return;

        Auth.SignOut();
        Debug.Log("[AuthManager] Signed out");
    }

    private void RaiseLoginFailed(string message)
    {
        Debug.LogError($"[AuthManager] {message}");
        OnLoginFailed?.Invoke(message);
    }
}
