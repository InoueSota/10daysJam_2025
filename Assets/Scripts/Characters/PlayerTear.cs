using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerTear : MonoBehaviour
{
    // 自コンポーネント
    private PlayerController controller;

    // フラグ類
    private bool isActive;

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

        // 十字ボタンの左右どちらかを押したら、左右どちらかを破り捨てる
        if (isActive && (Input.GetAxisRaw("Horizontal2") < 0f || Input.GetAxisRaw("Horizontal2") > 0f))
        {
            // 該当するFieldObjectを破る操作を行うが、破られるかどうかはAllFieldObjectManager内で判断する
            foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
            {
                if (Input.GetAxisRaw("Horizontal2") < 0f && fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
                else if (Input.GetAxisRaw("Horizontal2") > 0f && fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                {
                    fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                }
            }

            targetIntensity = 0f;
            isActive = false;
        }

        // Global Volume
        vignette.intensity.value += (targetIntensity - vignette.intensity.value) * (fadePower * Time.deltaTime);
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
