using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 카메라를 관리하는 클래스 (임시용)
/// </summary>
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private float r, pi;
    private float x, z, phi = Mathf.PI / 4;
    private float h, v, x_mov, z_mov;

    private float[] rotQuant = { 45f, 135f, 225f, 315f };
    private int currentRot, nextRot;
    private float rotDuration = .5f;
    private bool isRotating;

    [SerializeField]
    private float movSpeed, rotSpeed;

    [SerializeField]
    private float maxZoom, minZoom;
    Camera cam;
    private float zoom;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (!isRotating)
        {
            x += x_mov * movSpeed * Time.deltaTime * Mathf.Sqrt(zoom);
            z += z_mov * movSpeed * Time.deltaTime * Mathf.Sqrt(zoom);
        }

        zoom -= Input.mouseScrollDelta.y * 3;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = zoom;

        transform.position = new Vector3(x + r * Mathf.Cos(phi), -r * Mathf.Sin(pi), z + r * Mathf.Sin(phi));
        transform.LookAt(new Vector3(x, 0, z));
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        h = -input[0];
        v = -input[1];

        x_mov = v * Mathf.Cos(phi) + h * Mathf.Cos(phi - Mathf.PI / 2.0f);
        z_mov = v * Mathf.Sin(phi) + h * Mathf.Sin(phi - Mathf.PI / 2.0f);
    }

    void OnRotate(InputValue value)
    {
        float input = value.Get<float>();

        if (!isRotating && input != 0)
        {
            isRotating = true;

            if (input > 0) nextRot = (currentRot + 1) % rotQuant.Length;
            else nextRot = currentRot == 0 ? 3 : currentRot - 1;

            StartCoroutine("Rotate");
        }
    }

    IEnumerator Rotate()
    {
        float duration = 0f;

        while (duration < rotDuration)
        {
            phi = Mathf.LerpAngle(rotQuant[currentRot], rotQuant[nextRot], duration / rotDuration) * Mathf.Deg2Rad;
            duration += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        currentRot = nextRot;
        isRotating = false;
    }
}
