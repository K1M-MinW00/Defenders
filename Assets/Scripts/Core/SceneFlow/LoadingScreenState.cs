using System;
using System.Threading.Tasks;

public static class LoadingScreenState
{
    private static TaskCompletionSource<bool> readySource;

    public static string DestinationScene { get; private set; }
    public static float Progress { get; private set; }
    public static string ErrorMessage { get; private set; }

    public static event Action Changed;

    public static void Begin(string destinationScene)
    {
        DestinationScene = destinationScene;
        Progress = 0f;
        ErrorMessage = string.Empty;
        readySource = new TaskCompletionSource<bool>();
        Changed?.Invoke();
    }

    public static Task WaitUntilReadyAsync() =>
        readySource?.Task ?? Task.CompletedTask;

    public static void MarkReady() => readySource?.TrySetResult(true);

    public static void ReportProgress(float progress)
    {
        Progress = Math.Clamp(progress, 0f, 1f);
        Changed?.Invoke();
    }

    public static void ReportFailure(string message)
    {
        ErrorMessage = message ?? string.Empty;
        Changed?.Invoke();
    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        DestinationScene = string.Empty;
        Progress = 0f;
        ErrorMessage = string.Empty;
        readySource = null;
        Changed = null;
    }
}
