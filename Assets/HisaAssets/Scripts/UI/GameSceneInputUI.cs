using UnityEngine;

public class GameSceneInputUI : MonoBehaviour
{
    [SerializeField] PushButton[] controllerUIs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            controllerUIs[0].Push();
        }else if (Input.GetButtonUp("Jump"))
        {
            controllerUIs[0].Release();
        }
        if (Input.GetButtonDown("Special"))
        {
            controllerUIs[1].Push();
        }
        else if (Input.GetButtonUp("Special"))
        {
            controllerUIs[1].Release();
        }
        if (Input.GetButtonDown("Reset"))
        {
            controllerUIs[2].Push();
        }
        else if (Input.GetButtonUp("Reset"))
        {
            controllerUIs[2].Release();
        }
        if (Input.GetButtonDown("Undo"))
        {
            controllerUIs[3].Push();
        }
        else if (Input.GetButtonUp("Undo"))
        {
            controllerUIs[3].Release();
        }
        if (Input.GetButtonDown("Menu"))
        {
            controllerUIs[4].Push();
        }
        else if (Input.GetButtonUp("Menu"))
        {
            controllerUIs[4].Release();
        }
        if (Input.GetButtonDown("Cancel"))
        {
            controllerUIs[5].Push();
        }
        else if (Input.GetButtonUp("Cancel"))
        {
            controllerUIs[5].Release();
        }
    }
}
