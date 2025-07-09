using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : MonoBehaviour
{
    private void OnEnable()
    {
        InputHandler.Instance.OnEscapeInput += OnEscapeInput;
    }

    private void OnDisable()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnEscapeInput -= OnEscapeInput;
        }
    }

    private void OnEscapeInput()
    {
        UIManager.Instance.ProcessEscapeInput();
    }
}
