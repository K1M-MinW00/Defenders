using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

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


    public async Task<bool> InitializeAndLoginAsync()
    {
        if (IsBusy)
            return false;

        IsBusy = true;

        try
        {
            bool initOk = await InitializeFirebaseAsync();

            if (!initOk)
            {
                RaiseLoginFailed("Firebase initialization failed.");
                return false;
            }

            if (CurrentUser != null)
            {
                bool isSessionValid = await ValidateCurrentUserSessionAsync();

                if (isSessionValid)
                {
                    Debug.Log($"[AuthManager] Auto login Success. UID : {CurrentUser.UserId}");
                    OnLoginSucceeded?.Invoke(CurrentUser);
                    return true;
                }

                Debug.LogWarning("[AuthManager] Cached User session is invalid. Signing out");
                SignOut();
            }

            AuthResult authResult = await Auth.SignInAnonymouslyAsync();

            if (authResult == null || authResult.User == null)
            {
                RaiseLoginFailed("Anonymous login returned null user");
                return false;
            }

            Debug.Log($"[AuthManager] Anonymous login success. UID : {authResult.User.UserId}");
            OnLoginSucceeded?.Invoke(authResult.User);
            return true;
        }
        catch (Exception e)
        {
            RaiseLoginFailed($"로그인 처리 중 예외 발생 : {e}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> InitializeFirebaseAsync()
    {
        if (IsInitialized)
            return true;

        try
        {
            DependencyStatus dependecyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependecyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"[AuthManager] Firebase Dependency error : {dependecyStatus}");
                return false;
            }

            Auth = FirebaseAuth.DefaultInstance;
            IsInitialized = Auth != null;

            Debug.Log("[AuthManager] Firebase Auth Initiailzed");
            return IsInitialized;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AuthManager] Firebase initialize exception : {e}");
            return false;
        }
    }

    private async Task<bool> ValidateCurrentUserSessionAsync()
    {
        if (CurrentUser == null)
            return false;

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