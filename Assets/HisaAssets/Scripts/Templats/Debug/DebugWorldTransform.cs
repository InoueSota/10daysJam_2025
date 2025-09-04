using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWorldTransform : MonoBehaviour
{
    // 取得結果を保存する変数
    public Vector3 worldPosition;
    public Quaternion worldRotation;
    public Vector3 worldScale;

    // ContextMenu 属性をつけることで、インスペクターの右クリックメニューから実行可能に
    [ContextMenu("Update World Transform")]
    private void UpdateWorldTransform()
    {
        worldPosition = transform.position;
        worldRotation = transform.rotation;
        worldScale = transform.lossyScale;

        //Debug.Log("World transform updated!");
    }
}
