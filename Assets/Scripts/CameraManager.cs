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
    private float _followX, _followZ;
    private float _phi = Mathf.PI / 4;  // 현재 회전각
    private float _h, _v;               // 키보드 입력 값 저장용 변수
    private float _s;                   // 스크롤 입력 값 저장용 변수

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

        _x = MapManager.Instance.Tiles.GetLength(0) * 0.75f;
        _z = MapManager.Instance.Tiles.GetLength(1) * Mathf.Sqrt(3.0f) / 2f;

        _followX = _x;
        _followZ = _z;

        _currentZoom = 11.0f;

        InputHandler.Instance.OnMoveInput += OnMoveInput;
        InputHandler.Instance.OnScrollInput += OnScrollInput;
        InputHandler.Instance.OnRotateInput += OnRotateInput;
    }

    private void Update()
    {
        float deltaX = _v * Mathf.Cos(_phi) + _h * Mathf.Cos(_phi - Mathf.PI / 2.0f);
        float deltaY = _v * Mathf.Sin(_phi) + _h * Mathf.Sin(_phi - Mathf.PI / 2.0f);

        if (!_isLocked)
        {
            if (!_isRotating)
            {
                _x += deltaX * _moveSpeed * Time.deltaTime * Mathf.Sqrt(_currentZoom);
                _x = Mathf.Clamp(_x, 0.0f, MapManager.Instance.Tiles.GetLength(0) * 1.5f);

                _z += deltaY * _moveSpeed * Time.deltaTime * Mathf.Sqrt(_currentZoom);
                _z = Mathf.Clamp(_z, 0.0f, MapManager.Instance.Tiles.GetLength(1) * Mathf.Sqrt(3.0f));
            }

            _currentZoom = Mathf.Clamp(_currentZoom - _s * 3, _minZoom, _maxZoom);
            _camera.orthographicSize = _currentZoom;
        }

        _followX = Mathf.Lerp(_followX, _x, Time.deltaTime * 12.0f);
        _followZ = Mathf.Lerp(_followZ, _z, Time.deltaTime * 12.0f);
        transform.position = new Vector3(_followX + _r * Mathf.Cos(_phi), -_r * Mathf.Sin(_pi), _followZ + _r * Mathf.Sin(_phi));
        transform.LookAt(new Vector3(_followX, 0, _followZ));
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnMoveInput -= OnMoveInput;
            InputHandler.Instance.OnScrollInput -= OnScrollInput;
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

    private void OnScrollInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _s = input[1] / 120;
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
            _phi = Mathf.LerpAngle(_rotationValues[_currentRotation], _rotationValues[_nextRotation], EaseInOut(0.0f, 1.0f, elapsed / _rotationDuration)) * Mathf.Deg2Rad;
            elapsed += Time.deltaTime;

            yield return null;
        }

        _currentRotation = _nextRotation;
        _isRotating = false;
    }

    private float EaseInOut(float a, float b, float t)
    {
        t = -(Mathf.Cos(Mathf.PI * t) - 1.0f) / 2.0f;

        return a * (1.0f - t) + b * t;
    }
}
