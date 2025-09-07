using UnityEngine;

public class WarpManager : MonoBehaviour
{
    private bool isActive;

    void Start()
    {
        isActive = false;
    }

    void Update()
    {
        // Warpがステージに２つ以上あればWarpが可能（＝isActiveがtrueになる）
        int warpCount = 0;

        foreach (GameObject warp in GameObject.FindGameObjectsWithTag("FieldObject"))
        {
            if (warp.GetComponent<AllFieldObjectManager>().GetObjectType() == AllFieldObjectManager.ObjectType.WARP)
            {
                // 該当Objectの位置をビューポート座標に変換
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(warp.transform.position);

                // 画面内チェック（0～1の範囲）
                if (viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1) { warpCount++; }
            }
        }

        if (1 < warpCount)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
    }

    // Setter
    public void SetWarpPosition(ref Vector3 _warpPosition, ref GameObject _warpObj)
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
        if (nearWarp) { _warpPosition = nearWarp.GetComponent<AllFieldObjectManager>().GetCurrentPosition(); _warpObj = nearWarp; }
    }
}
