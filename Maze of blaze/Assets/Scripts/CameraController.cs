using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float maxY = 80;
    float minY = -80;

    [Tooltip("Horizontal rotation speed")]
    public float RotationSpeedX = 5f;
    [Tooltip("Vertical rotation speed")]
    public float RotationSpeedY = 5f;

    [Tooltip("Coefficient by which rotation speed should be multiplied when using a stick")]
    public float RotationSpeedSticksModifier = 20f;

    float vRotation = 0f;
    float hRotation = 0f;
    public void Start()
    {
        if (GameManager.instance.cameraType == GameManager.CameraType.FIRST_PERSON)
        {
            InputManager.instance.lookCallback += Rotate;
        }
        if (GameManager.instance.controlType == GameManager.ControlType.STICKS) {
            RotationSpeedX *= RotationSpeedSticksModifier;
            RotationSpeedY *= RotationSpeedSticksModifier;
        }
        
    }

    public void Rotate(Vector2 input)
    {
        hRotation -= input.y*Time.deltaTime* RotationSpeedX;
        vRotation += input.x*Time.deltaTime * RotationSpeedY;
        hRotation = Mathf.Clamp(hRotation, minY, maxY);
        vRotation %= 360.0f;
        transform.localRotation = Quaternion.Euler(hRotation, vRotation, 0);
    }
}
