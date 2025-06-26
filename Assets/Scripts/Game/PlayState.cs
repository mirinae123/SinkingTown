using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayState : MonoBehaviour
{
    private Camera _mainCamera;
    private Vector2 _mousePosition;
    private bool _isPointerOverGameObject = false;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        InputHandler.Instance.OnPointMoveInput += OnPointMoveInput;
        InputHandler.Instance.OnClickInput += OnClickInput;
        InputHandler.Instance.OnEscapeInput += OnEscapeInput;
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnPointMoveInput -= OnPointMoveInput;
        InputHandler.Instance.OnClickInput -= OnClickInput;
        InputHandler.Instance.OnEscapeInput -= OnEscapeInput;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPaused)
        {
            GameManager.Instance.UpdateTime();
            GameManager.Instance.ProcessOceanRise();
        }

        _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();
    }

    private void OnPointMoveInput(InputValue value)
    {
        _mousePosition = value.Get<Vector2>();
    }

    private void OnClickInput()
    {
        if (Physics.Raycast(_mainCamera.ScreenPointToRay(_mousePosition), out RaycastHit hit, Mathf.Infinity))
        {
            if (!_isPointerOverGameObject)
            {
                Vector2Int tilePos = HexaUtility.GetTileCoordinate(hit.point);
                UIManager.Instance.ShowTileInfo(tilePos);
            }
        }
    }

    private void OnEscapeInput()
    {
        UIManager.Instance.ProcessEscapeInput();
    }
}
