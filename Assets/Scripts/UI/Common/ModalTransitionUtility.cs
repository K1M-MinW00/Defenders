using System;
using UnityEngine;

public static class ModalTransitionUtility
{
    public static void RequestExit(GameObject modalRoot, Action immediateClose)
    {
        if (immediateClose == null) return;
        if (modalRoot == null || !modalRoot.activeInHierarchy)
        {
            immediateClose();
            return;
        }

        ModalTransition transition = modalRoot.GetComponent<ModalTransition>();
        if (transition == null || transition.IsInvokingExitCallback)
        {
            immediateClose();
            return;
        }

        transition.PlayExit(immediateClose);
    }
}
