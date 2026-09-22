using UnityEngine;

[DisallowMultipleComponent]
public sealed class PopupBackInput : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PopupBackStack.TryCloseTop();
    }
}
