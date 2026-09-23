using UnityEngine;

public sealed class SimplePopupView : MonoBehaviour
{
    public void Open() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);
}
