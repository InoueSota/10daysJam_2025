using UnityEngine;

public class CrabManager : MonoBehaviour
{
    public enum ThrowDirection { RIGHT, UP, LEFT, DOWN }
    private ThrowDirection throwDirection = ThrowDirection.LEFT;

    [SerializeField] private CrabSpriteScript crabSpriteScript;
    private SoundInstantiateScript sound;

    void Start()
    {
        sound = GetComponent<SoundInstantiateScript>();
    }

    // Getter
    public Vector3 GetThrowVector()
    {
        switch (throwDirection)
        {
            case ThrowDirection.RIGHT:
                return Vector3.right;
            case ThrowDirection.UP:
                return Vector3.up;
            case ThrowDirection.LEFT:
                return Vector3.left;
            case ThrowDirection.DOWN:
                return Vector3.down;
        }
        return Vector3.zero;
    }
    public ThrowDirection GetThrowDirection() { return throwDirection; }

    public CrabSpriteScript GetSpriteScript() { return crabSpriteScript; }

    // Setter
    public void SetThrowDirection(ThrowDirection _throwDirection) { throwDirection = _throwDirection; crabSpriteScript.ChangeDirection((int)_throwDirection); }
    public void StartSound()
    {
        sound.PlaySound(Random.Range(0, 99) % 3, 0.7f);
    }
}
