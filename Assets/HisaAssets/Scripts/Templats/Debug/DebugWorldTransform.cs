using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWorldTransform : MonoBehaviour
{
    // �擾���ʂ�ۑ�����ϐ�
    public Vector3 worldPosition;
    public Quaternion worldRotation;
    public Vector3 worldScale;

    // ContextMenu ���������邱�ƂŁA�C���X�y�N�^�[�̉E�N���b�N���j���[������s�\��
    [ContextMenu("Update World Transform")]
    private void UpdateWorldTransform()
    {
        worldPosition = transform.position;
        worldRotation = transform.rotation;
        worldScale = transform.lossyScale;

        //Debug.Log("World transform updated!");
    }
}
