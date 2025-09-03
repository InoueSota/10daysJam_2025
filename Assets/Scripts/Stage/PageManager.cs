using UnityEngine;

public class PageManager : MonoBehaviour
{
    // ƒtƒ‰ƒO—Þ
    private bool isSetLeftTear;
    private bool isSetRightTear;

    private Vector3 saveLeftTearPosition;
    private Vector3 saveRightTearPosition;

    void Start()
    {
        isSetLeftTear = false;
        isSetRightTear = false;
    }

    void Update()
    {
        
    }

    // Getter
    public bool IsNoPagePosition(Vector3 _objectPosition)
    {
        if (isSetLeftTear && _objectPosition.x < saveLeftTearPosition.x)
        {
            return true;
        }
        else if (isSetRightTear && _objectPosition.x > saveLeftTearPosition.x)
        {
            return true;
        }
        return false;
    }

    // Setter
    public void SetTearInfomation(Vector3 _tearPosition, Vector3 _direction)
    {
        if (_direction.x < 0f)
        {
            if (!isSetLeftTear || (isSetLeftTear && _tearPosition.x > saveLeftTearPosition.x))
            {
                saveLeftTearPosition = _tearPosition;
                isSetLeftTear = true;
            }
        }
        else
        {
            if (!isSetRightTear || (isSetRightTear && _tearPosition.x < saveRightTearPosition.x))
            {
                saveRightTearPosition = _tearPosition;
                isSetRightTear = true;
            }
        }
    }
}
