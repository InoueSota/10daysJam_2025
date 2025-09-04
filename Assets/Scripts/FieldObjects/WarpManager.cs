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
        if (nearWarp) { _playerTransform.position = nearWarp.transform.position; _warpObj = nearWarp; }
    }
}
