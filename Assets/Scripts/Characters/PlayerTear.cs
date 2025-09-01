using UnityEngine;

public class PlayerTear : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // フラグ類
    private bool isActive;

    public void Initialize(PlayerController _controller)
    {
        // 自コンポーネントの取得
        controller = _controller;
    }

    void Update()
    {
        // 破り、開始
        if (!isActive && controller.IsGrounded() && Input.GetButtonDown("Special"))
        {
            controller.SetDefault();
            isActive = true;
        }

        // 十字ボタンの左右どちらかを押したら、左右どちらかを破り捨てる
        if (isActive && (Input.GetAxis("Horizontal2") < 0f || Input.GetAxis("Horizontal2") > 0f))
        {
            // 該当するFieldObjectを破る操作を行うが、破られるかどうかはAllFieldObjectManager内で判断する
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (Input.GetAxis("Horizontal2") < 0f && fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
                else if (Input.GetAxis("Horizontal2") > 0f && fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
            }

            isActive = false;
        }
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
