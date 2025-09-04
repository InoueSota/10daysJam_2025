using UnityEngine;

public class WarpManager : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    // Setter
    public void DoWarp(Transform _playerTransform, ref GameObject _warpObj)
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
        if (nearWarp) { _playerTransform.position = nearWarp.transform.position; _warpObj = nearWarp; }
    }
}
