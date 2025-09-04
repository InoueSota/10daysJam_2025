using UnityEngine;

public class CancelParentRotation : MonoBehaviour
{
    // この回転を外部から設定することで、自分自身の回転が可能
    public Quaternion selfRotation = Quaternion.identity;

    void LateUpdate()
    {
        if (transform.parent != null)
        {
            // 親の回転を打ち消したうえで、自分自身の回転を加える
            transform.rotation = transform.parent.rotation * selfRotation;
        }
        else
        {
            transform.rotation = selfRotation;
        }
    }

    // 回転を角度で設定したい場合の補助メソッド
    public void SetSelfEuler(Vector3 euler)
    {
        selfRotation = Quaternion.Euler(euler);
    }
}
