using UnityEngine;

public class InputDeviceDetector : MonoBehaviour
{
    public enum InputDevice { Keyboard, Controller }
    public static InputDevice CurrentDevice { get; private set; } = InputDevice.Keyboard;
    public static InputDevice preDevice;

    [Header("デッドゾーン（コントローラー用）")]
    [SerializeField] private float axisDeadZone = 0.2f;

    void Update()
    {
        preDevice = CurrentDevice;
        // --- キーボード検知 ---
        if (Input.anyKeyDown)
        {
            if (CurrentDevice != InputDevice.Keyboard)
            {
                CurrentDevice = InputDevice.Keyboard;
                Debug.Log("切替: Keyboard");
            }
        }

        // --- コントローラー検知 ---
        // ※ InputManager に JoystickX / JoystickY を追加してある前提
        float joyX = Input.GetAxis("JoystickX");
        float joyY = Input.GetAxis("JoystickY");

        if (Mathf.Abs(joyX) > axisDeadZone || Mathf.Abs(joyY) > axisDeadZone ||
            IsAnyJoystickButtonDown())
        {
            if (CurrentDevice != InputDevice.Controller)
            {
                CurrentDevice = InputDevice.Controller;
                Debug.Log("切替: Controller");
            }
        }
    }

    bool IsAnyJoystickButtonDown()
    {
        // 任意のジョイスティック(1..8)のボタン(0..19)を総当たり
        for (int joy = 1; joy <= 8; joy++)
        {
            for (int btn = 0; btn <= 19; btn++)
            {
                var kc = (KeyCode)System.Enum.Parse(typeof(KeyCode), $"Joystick{joy}Button{btn}");
                if (Input.GetKeyDown(kc)) return true;
            }
        }

        // “全ジョイスティック共通”キーコード（環境によっては不要）
        for (int btn = 0; btn <= 19; btn++)
        {
            var kc = (KeyCode)System.Enum.Parse(typeof(KeyCode), $"JoystickButton{btn}");
            if (Input.GetKeyDown(kc)) return true;
        }

        return false;
    }
}
