using UnityEngine;

public class WarpManager : MonoBehaviour
{
    // Setter
    public void SetWarpPosition(ref Vector3 _warpPosition, ref GameObject _warpObj)
    {
        GameObject nearWarp = null;

        // ���̃��[�v�i�ł��߂����[�v�j��T��
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

        // �v���C���[�����[�v������
        if (nearWarp) { _warpPosition = nearWarp.transform.position; _warpObj = nearWarp; }
    }
}
