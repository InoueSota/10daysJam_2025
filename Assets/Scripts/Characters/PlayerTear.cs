using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTear : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // フラグ類
    private bool isActive;
    private bool isTear;

    // Global Volume
    [SerializeField] private float fadePower;
    [SerializeField] private Volume postEffectVolume;
    private Vignette vignette;
    private float maxIntensity = 0.5f;
    private float targetIntensity = 0f;

    public void Initialize(PlayerController _controller)
    {
        // 自コンポーネントの取得
        controller = _controller;

        // Global Volume
        postEffectVolume.profile.TryGet(out vignette);
    }

    public void ManualUpdate()
    {
        // 破り、開始
        if (!isActive && controller.IsGrounded() && Input.GetButtonDown("Special"))
        {
            controller.SetDefault();
            targetIntensity = maxIntensity;
            isActive = true;
        }
        // 破り、終了
        else if (isActive && Input.GetButtonDown("Special"))
        {
            controller.SetBackToNormal();
            targetIntensity = 0f;
            isActive = false;
        }

        // 十字ボタンの左右どちらかを押したら、左右どちらかを破り捨てる
        if (isActive && !isTear && (Input.GetAxisRaw("Horizontal") < 0f || Input.GetAxisRaw("Horizontal") > 0f))
        {
            // 該当するFieldObjectを破る操作を行うが、破られるかどうかはAllFieldObjectManager内で判断する
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (Input.GetAxisRaw("Horizontal") < 0f && fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
                else if (Input.GetAxisRaw("Horizontal") > 0f && fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
            }

            isTear = true;
        }

        // 左スティックを離したら再入力を受け付けるようにする
        if (isTear && Input.GetAxisRaw("Horizontal") == 0f) { isTear = false; }

        // Global Volume
        vignette.intensity.value += (targetIntensity - vignette.intensity.value) * (fadePower * Time.deltaTime);
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
