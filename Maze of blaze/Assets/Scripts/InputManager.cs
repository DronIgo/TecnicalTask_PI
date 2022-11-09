using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input manager is used to get information from new Input System
/// </summary>
public class InputManager : MonoBehaviour
{
    [HideInInspector]
    public Vector2 movementDirection;
    [HideInInspector]
    public Vector2 lookDirection;

    [HideInInspector]
    public static InputManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    bool fpsCamera;
    Transform fpsCameraTransform;
    private void Start()
    {
        fpsCamera = GameManager.instance.cameraType == GameManager.CameraType.FIRST_PERSON;
        if (fpsCamera)
            fpsCameraTransform = GameManager.instance.camera.transform;
    }

    [HideInInspector]
    public CharacterController playerController;

    /// <summary>
    /// Reads and stores the movement input
    /// </summary>
    public void GetMovementInput(InputAction.CallbackContext callbackContext)
    {
        movementDirection = callbackContext.ReadValue<Vector2>();

        
        if (movementDirection.magnitude > 1)
            movementDirection = movementDirection.normalized;
    }

    [HideInInspector]
    public float pauseButton = 0;

    /// <summary>
    /// Collects pause button input
    /// </summary>
    public void GetPauseInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            GameManager.instance.TogglePause();
        }
    }

    public delegate void LookAroundEvent(Vector2 direction);

    public event LookAroundEvent lookCallback;

    /// <summary>
    /// Collects look round input
    /// </summary>
    public void GetMouseLookInput(InputAction.CallbackContext callbackContext)
    {
        lookDirection = callbackContext.ReadValue<Vector2>();
    }

    public delegate void MovementEvent(Vector2 direction);

    public event MovementEvent movementCallback;
    public void Update()
    {
        Vector2 movementDirectionAdj = movementDirection;
        //Adjust movement direction according to camera 
        if (fpsCamera)
        {
            Vector3 inputDirection = new Vector3(movementDirection.x, 0, movementDirection.y);

            //Get the camera horizontal rotation
            Vector3 faceDirection = new Vector3(fpsCameraTransform.forward.x, 0, fpsCameraTransform.forward.z);

            //Get the angle between world forward and camera
            float cameraAngle = Vector3.SignedAngle(Vector3.forward, faceDirection, Vector3.up);

            //Finally rotate the input direction horizontally by the cameraAngle
            Vector3 moveDirection = Quaternion.Euler(0, cameraAngle, 0) * inputDirection;
            movementDirectionAdj = new Vector2(moveDirection.x, moveDirection.z);
        }

        if (movementCallback != null)
            movementCallback(movementDirectionAdj);
        if (lookCallback != null)
            lookCallback(lookDirection);
    }
}
