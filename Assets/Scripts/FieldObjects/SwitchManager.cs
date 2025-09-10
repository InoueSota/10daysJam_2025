using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    // スプライト画像
    [SerializeField] private Sprite[] sprites;

    // 自コンポーネント
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;
    private SoundInstantiateScript sound;

    // ON OFF
    public enum Status { ON, OFF }
    private Status status = Status.ON;

    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sound = GetComponent<SoundInstantiateScript>();
    }

    // Getter
    public Status GetStatus() { return status; }

    // Setter
    public void SetStatus(Status _status)
    {
        switch (_status)
        {
            case Status.ON:

                boxCollider2D.enabled = true;
                spriteRenderer.sprite = sprites[0];
                sound.PlaySound(0, 0.4f);

                break;
            case Status.OFF:

                boxCollider2D.enabled = false;
                spriteRenderer.sprite = sprites[1];
                sound.PlaySound(1, 0.4f);

                break;
        }
        status = _status;
    }
}
