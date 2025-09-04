using UnityEngine;

public class DontDestroyOnLoadObject : MonoBehaviour
{
    private void Awake()
    {
        // すでに同じオブジェクトが存在するなら破棄する（重複防止）
        if (FindObjectsOfType<DontDestroyOnLoadObject>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // このオブジェクトをシーン切り替え時に破棄しないようにする
        DontDestroyOnLoad(gameObject);
    }


}
