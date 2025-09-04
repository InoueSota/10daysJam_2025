using UnityEngine;

public class FindRoot : MonoBehaviour
{
    // 一番上の親を返すメソッド
    public static Transform GetRoot(Transform current)
    {
        while (current.parent != null)
        {
            current = current.parent;
        }
        return current;
    }

    // 例：Startで自分のルートを表示する
    void Start()
    {
        Transform root = GetRoot(transform);
        Debug.Log("Root GameObject: " + root.name);
    }
}
