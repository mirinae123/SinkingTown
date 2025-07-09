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
    [SerializeField]
    private float _r, _pi;

    [SerializeField]
    private float _moveSpeed, _rotationSpeed;

    [SerializeField]
    private float _maxZoom, _minZoom;

    private float _x, _z, _phi = Mathf.PI / 4;
    private float _h, _v, _deltaX, _deltaZ;

    private float[] _rotationValues = { 45f, 135f, 225f, 315f };
    private float _rotationDuration = .5f;

    private Camera _camera;

    private int _currentRotation, _nextRotation;
    private bool _isRotating;
    private float _currentZoom; 

    private void Start()
    {
        _camera = GetComponent<Camera>();

        InputHandler.Instance.OnMoveInput += OnMoveInput;
        InputHandler.Instance.OnRotateInput += OnRotateInput;
    }

    private void Update()
    {
        if (!_isRotating)
        {
            _x += _deltaX * _moveSpeed * Time.deltaTime * Mathf.Sqrt(_currentZoom);
            _z += _deltaZ * _moveSpeed * Time.deltaTime * Mathf.Sqrt(_currentZoom);
        }

        _deltaX = _v * Mathf.Cos(_phi) + _h * Mathf.Cos(_phi - Mathf.PI / 2.0f);
        _deltaZ = _v * Mathf.Sin(_phi) + _h * Mathf.Sin(_phi - Mathf.PI / 2.0f);

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

    private void OnMoveInput(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _h = -input[0];
        _v = -input[1];
    }

    private void OnRotateInput(InputValue value)
    {
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
