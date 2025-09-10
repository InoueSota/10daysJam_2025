using UnityEngine;

public class WarpManager : MonoBehaviour
{
    private SoundInstantiateScript sound;

    void Start()
    {
        sound = GetComponent<SoundInstantiateScript>();
    }

    // Setter
    public void SetWarpPosition(ref Vector3 _warpPosition, ref GameObject _warpObj)
    {
        GameObject nearWarp = null;

        // 他のワープ（最も近いワープ）を探す
        foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
        {
            if (gameObject != fieldObject && fieldObject.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                if (!nearWarp || (nearWarp && Vector3.Distance(transform.position, nearWarp.transform.position) > Vector3.Distance(transform.position, fieldObject.transform.position)))
                {
                    nearWarp = fieldObject;
                }
            }
        }

        // プレイヤーをワープさせる
        if (nearWarp) { _warpPosition = nearWarp.GetComponent<AllFieldObjectManager>().GetCurrentPosition(); _warpObj = nearWarp; }
    }
    public void StartSound()
    {
        sound.PlaySound(0, 0.3f);
    }
}
