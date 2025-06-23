using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : SingletonBehaviour<InputHandler>
{
    public UnityAction<InputValue> OnMoveInput;
    public UnityAction<InputValue> OnRotateInput;
    public UnityAction<InputValue> OnPointMoveInput;
    public UnityAction OnClickInput;
    public UnityAction OnEscapeInput;

    private void OnMove(InputValue inputValue)
    {
        OnMoveInput?.Invoke(inputValue);
    }

    private void OnRotate(InputValue inputValue)
    {
        OnRotateInput?.Invoke(inputValue);
    }

    private void OnPointMove(InputValue inputValue)
    {
        OnPointMoveInput?.Invoke(inputValue);
    }

    private void OnClick()
    {
        OnClickInput?.Invoke();
    }

    private void OnEscape()
    {
        OnEscapeInput?.Invoke();
    }
}
