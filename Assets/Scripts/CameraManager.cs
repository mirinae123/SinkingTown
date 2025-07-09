using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 카메라를 관리하는 클래스
/// </summary>
public class CameraManager : SingletonBehaviour<CameraManager>
{
    [SerializeField] private float _r;   // 초점으로부터의 거리
    [SerializeField] private float _pi;  // 내려다보는 각도

    [SerializeField]
    private float _moveSpeed, _rotationSpeed;

    [SerializeField]
    private float _maxZoom, _minZoom;

    private float _x, _z;               // 초점 위치
    private float _phi = Mathf.PI / 4;  // 현재 회전각
    private float _h, _v;               // 입력 값 저장용 변수

    private float[] _rotationValues = { 45f, 135f, 225f, 315f };    // 회전에 사용할 각도 값들
    private float _rotationDuration = 0.5f;                         // 회전에 걸리는 시간

    private Camera _camera;

    private int _currentRotation, _nextRotation;
    private float _currentZoom;

    private bool _isRotating = false;
    private bool _isLocked = false;

    private void Start()
    {
        _camera = GetComponent<Camera>();

        InputHandler.Instance.OnMoveInput += OnMoveInput;
        InputHandler.Instance.OnRotateInput += OnRotateInput;
    }

    private void Update()
    {
        float deltaX = _v * Mathf.Cos(_phi) + _h * Mathf.Cos(_phi - Mathf.PI / 2.0f);
        float deltaY = _v * Mathf.Sin(_phi) + _h * Mathf.Sin(_phi - Mathf.PI / 2.0f);

        if (!_isRotating && !_isLocked)
        {
            _x += deltaX * _moveSpeed * Time.deltaTime * Mathf.Sqrt(_currentZoom);
            _z += deltaY * _moveSpeed * Time.deltaTime * Mathf.Sqrt(_currentZoom);
        }

        _currentZoom = Mathf.Clamp(_currentZoom - Input.mouseScrollDelta.y * 3, _minZoom, _maxZoom);
        _camera.orthographicSize = _currentZoom;

        transform.position = new Vector3(_x + _r * Mathf.Cos(_phi), -_r * Mathf.Sin(_pi), _z + _r * Mathf.Sin(_phi));
        transform.LookAt(new Vector3(_x, 0, _z));
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnMoveInput -= OnMoveInput;
            InputHandler.Instance.OnRotateInput -= OnRotateInput;
        }
    }
    
    public void LockCamera()
    {
        _isLocked = true;
    }

    public void UnlockCamera()
    {
        _isLocked = false;
    }

    private void OnMoveInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _h = -input[0];
        _v = -input[1];
    }

    private void OnRotateInput(InputValue value)
    {
        if (_isLocked)
        {
            return;
        }

        float input = value.Get<float>();

        if (!_isRotating && input != 0)
        {
            _isRotating = true;

            if (input > 0) _nextRotation = (_currentRotation + 1) % _rotationValues.Length;
            else _nextRotation = _currentRotation == 0 ? 3 : _currentRotation - 1;

            StartCoroutine("Rotate");
        }
    }

    private IEnumerator Rotate()
    {
        float elapsed = 0f;

        while (elapsed < _rotationDuration)
        {
            _phi = Mathf.LerpAngle(_rotationValues[_currentRotation], _rotationValues[_nextRotation], elapsed / _rotationDuration) * Mathf.Deg2Rad;
            elapsed += Time.deltaTime;

            yield return null;
        }

        _currentRotation = _nextRotation;
        _isRotating = false;
    }
}
