using UnityEngine;

public class CrabManager : MonoBehaviour
{
    public enum ThrowDirection { RIGHT, UP, LEFT, DOWN }
    private ThrowDirection throwDirection = ThrowDirection.LEFT;

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

    // Setter
    public void SetThrowDirection(ThrowDirection _throwDirection) { throwDirection = _throwDirection; }
}
