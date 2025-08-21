using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : MonoBehaviour
{
    private void OnEnable()
    {
        InputHandler.Instance.OnEscapeInput += OnEscapeInput;
        CameraManager.Instance.LockCamera();
    }

    private void OnDisable()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnEscapeInput -= OnEscapeInput;
        }

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.UnlockCamera();
        }
    }

    private void OnEscapeInput()
    {
        if (UIManager.Instance.CurrentPanelType != PanelType.End &&
            UIManager.Instance.CurrentPanelType != PanelType.Loading)
        {
            UIManager.Instance.HidePanel();
        }
    }
}
