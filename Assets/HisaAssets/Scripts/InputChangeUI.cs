using UnityEngine;

public class InputChangeUI : MonoBehaviour
{

    [SerializeField] GameObject[] keybordUI;
    [SerializeField] GameObject[] controlUI;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(InputDeviceDetector.CurrentDevice== InputDeviceDetector.InputDevice.Keyboard)
        {
            foreach (var ui in keybordUI)
            {
                ui.SetActive(true);
            }
            foreach (var ui in controlUI)
            {
                ui.SetActive(false);
            }
        }
        else if (InputDeviceDetector.CurrentDevice == InputDeviceDetector.InputDevice.Controller)
        {
            foreach (var ui in keybordUI)
            {
                ui.SetActive(false);
            }
            foreach (var ui in controlUI)
            {
                ui.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (InputDeviceDetector.CurrentDevice != InputDeviceDetector.preDevice) {

            if (InputDeviceDetector.CurrentDevice == InputDeviceDetector.InputDevice.Keyboard)
            {
                foreach (var ui in keybordUI)
                {
                    ui.SetActive(true);
                }
                foreach (var ui in controlUI)
                {
                    ui.SetActive(false);
                }
            }
            else if (InputDeviceDetector.CurrentDevice == InputDeviceDetector.InputDevice.Controller)
            {
                foreach (var ui in keybordUI)
                {
                    ui.SetActive(false);
                }
                foreach (var ui in controlUI)
                {
                    ui.SetActive(true);
                }
            }
        }
    }
}
