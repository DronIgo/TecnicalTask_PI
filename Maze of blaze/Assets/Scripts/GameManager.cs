using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// GameManager contains information about game state as well as refernces to different objects used in the project
/// </summary>
public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public static GameManager instance;
    [HideInInspector]
    public Player player;
    [HideInInspector]
    public MazeData mazeData;

    [HideInInspector]
    public GameObject camera;

    [Header("Should be set up in the editor")]
    public GameObject TopDownCamera;
    public GameObject FirstPersonCamera;
    public PlayerInput playerInput;
    // Whether the application is paused
    private bool isPaused = false;

    // Wheter the player is dead
    private bool isDead = false;

    [HideInInspector]
    public UIManager uiManager;
    [HideInInspector]
    public EnemyManager enemyManager;

    public enum ControlType {
        MOUSE_KEYBOARD,
        STICKS,
    };
    [Tooltip("Which control scheme should be used")]
    public ControlType controlType = ControlType.MOUSE_KEYBOARD;

    public enum CameraType
    {
        TOPDOWN,
        FIRST_PERSON,
    };
    [Tooltip("Which camera should be used")]
    public CameraType cameraType = CameraType.TOPDOWN;

    private void Awake()
    {
        //We will utilise Singleton for GameManager
        isDead = false;
        if (instance == null)
            instance = this;

        //set up references
        player = GameObject.FindObjectOfType<Player>();
        uiManager = GameObject.FindObjectOfType<UIManager>();
        enemyManager = GameObject.FindObjectOfType<EnemyManager>();
        mazeData = GameObject.FindObjectOfType<MazeData>();


        //set up camera and controls
        switch (cameraType) {
            case CameraType.TOPDOWN:
                FirstPersonCamera.SetActive(false);
                TopDownCamera.SetActive(true);
                camera = TopDownCamera;
                break;
            case CameraType.FIRST_PERSON:
                FirstPersonCamera.SetActive(true);
                TopDownCamera.SetActive(false);
                camera = FirstPersonCamera;
                break;
        }

        switch (controlType)
        {
            case ControlType.MOUSE_KEYBOARD:
                playerInput.SwitchCurrentActionMap("PlayerKeyboard");
                break;
            case ControlType.STICKS:
                playerInput.SwitchCurrentActionMap("PlayerSticks");
                break;
        }

        //subscribe UIManagers UIUpdate to player health change event
        player.healthChangeCallback += uiManager.UpdateUI;

        //subscribe EnemyManager to player position change event
        player.positionChangeCallback += enemyManager.UpdatePlayerInformation;

        if ((controlType == ControlType.MOUSE_KEYBOARD) && (cameraType == CameraType.FIRST_PERSON))
            Cursor.lockState = CursorLockMode.Locked;

        Pause(false);
    }

    /// <summary>
    /// Changes the Pause state to the opposite one
    /// </summary>
    public void TogglePause()
    {
        if (!isDead)
            Pause(!isPaused);
    }

    /// <summary>
    /// Pauses (or unpauses) the game
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        isPaused = pause;
        if (pause)
        {
            uiManager.GoToPage(uiManager.pausePage);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        } else
        {
            uiManager.GoToPage(uiManager.defaultPage);
            Time.timeScale = 1;
            if ((controlType == ControlType.MOUSE_KEYBOARD) && (cameraType == CameraType.FIRST_PERSON))
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// GameOver is called from Player Health through Unity Events
    /// </summary>
    public void GameOver()
    {
        Time.timeScale = 0;
        uiManager.GoToPage(uiManager.gameOverPage);
        isDead = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
