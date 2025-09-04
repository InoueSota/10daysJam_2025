using UnityEngine;

public class FindRoot : MonoBehaviour
{
    // ��ԏ�̐e��Ԃ����\�b�h
    public static Transform GetRoot(Transform current)
    {
        while (current.parent != null)
        {
            current = current.parent;
        }
        return current;
    }

    // ��FStart�Ŏ����̃��[�g��\������
    void Start()
    {
        Transform root = GetRoot(transform);
        Debug.Log("Root GameObject: " + root.name);
    }
}
