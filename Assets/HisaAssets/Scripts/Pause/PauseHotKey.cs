// PauseHotkey.cs
using UnityEngine;

public class PauseHotkey : MonoBehaviour
{
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] bool xboxStartButton = true; // JoystickButton7

    void Update()
    {
        if (Input.GetKeyDown(pauseKey)) PauseService.TogglePause(this);
        else if (xboxStartButton && Input.GetKeyDown(KeyCode.JoystickButton7))
            PauseService.TogglePause(this);
    }
}
