using UnityEngine;

public class CancelParentRotation : MonoBehaviour
{
    // ���̉�]���O������ݒ肷�邱�ƂŁA�������g�̉�]���\
    public Quaternion selfRotation = Quaternion.identity;

    void LateUpdate()
    {
        if (transform.parent != null)
        {
            // �e�̉�]��ł������������ŁA�������g�̉�]��������
            transform.rotation = transform.parent.rotation * selfRotation;
        }
        else
        {
            transform.rotation = selfRotation;
        }
    }

    // ��]���p�x�Őݒ肵�����ꍇ�̕⏕���\�b�h
    public void SetSelfEuler(Vector3 euler)
    {
        selfRotation = Quaternion.Euler(euler);
    }
}
