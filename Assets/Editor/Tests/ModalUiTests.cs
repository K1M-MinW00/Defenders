using NUnit.Framework;
using UnityEngine;

public sealed class ModalDismissHandlerSpy : MonoBehaviour, IModalDismissHandler
{
    public int DismissCount { get; private set; }

    public void Dismiss() => DismissCount++;
}

public sealed class ModalUiTests
{
    [Test]
    public void SystemBack_RoutesToDismissHandlerExactlyOnce()
    {
        GameObject overlay = new("TestOverlay");
        try
        {
            ModalDismissHandlerSpy handler = overlay.AddComponent<ModalDismissHandlerSpy>();
            ModalBackdropDismiss dismiss = overlay.AddComponent<ModalBackdropDismiss>();
            dismiss.Configure(null, overlay, handler);

            dismiss.DismissFromSystemBack();

            Assert.That(handler.DismissCount, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(overlay);
        }
    }

    [Test]
    public void SystemBack_DeactivatesSimpleOverlayWithoutHandler()
    {
        GameObject overlay = new("TestOverlay");
        try
        {
            ModalBackdropDismiss dismiss = overlay.AddComponent<ModalBackdropDismiss>();
            dismiss.Configure(null, overlay);

            dismiss.DismissFromSystemBack();

            Assert.That(overlay.activeSelf, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(overlay);
        }
    }
}
