using UnityEngine;

public class DontDestroyOnLoadObject : MonoBehaviour
{
    private void Awake()
    {
        // ���łɓ����I�u�W�F�N�g�����݂���Ȃ�j������i�d���h�~�j
        if (FindObjectsOfType<DontDestroyOnLoadObject>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // ���̃I�u�W�F�N�g���V�[���؂�ւ����ɔj�����Ȃ��悤�ɂ���
        DontDestroyOnLoad(gameObject);
    }


}
