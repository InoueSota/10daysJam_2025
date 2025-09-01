using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{
    public enum ObjectType
    {
        GROUND,
        BLOCK
    }
    [SerializeField] private ObjectType objectType;

    public enum Status
    {
        FIRST,
        SECOND
    }
    private Status status = Status.FIRST;

    // 自コンポーネント
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;

    void Start()
    {
        // 自コンポーネントの取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        switch (objectType)
        {
            case ObjectType.GROUND:



                break;
            case ObjectType.BLOCK:

                if (transform.parent.gameObject.layer == 6)
                {
                    spriteRenderer.color = Color.yellow;
                    status = Status.FIRST;
                }
                else if (transform.parent.gameObject.layer == 7)
                {
                    spriteRenderer.color = Color.yellow;
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    boxCollider2D.enabled = false;
                    status = Status.SECOND;
                }

                break;
        }
    }

    void Update()
    {

    }

    public void HitTear()
    {
        switch (objectType)
        {
            case ObjectType.GROUND:
                break;
            case ObjectType.BLOCK:

                switch (status)
                {
                    case Status.FIRST:

                        Destroy(gameObject);

                        break;
                    case Status.SECOND:

                        spriteRenderer.color = Color.yellow;
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;

                        break;
                }

                break;
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
    public Status GetStatus() { return status; }
}
