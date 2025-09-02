using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{

    //レイヤーごとの色
    [SerializeField] Color[] layerColor = new Color[3];

    // 該当するObjectType
    public enum ObjectType
    {
        GROUND,
        GOAL,
        BLOCK
    }
    [SerializeField] private ObjectType objectType;

    // 該当する表示状態
    public enum Status
    {
        FIRST,
        SECOND,
        THIRD
    }
    private Status status = Status.FIRST;

    // 自コンポーネント
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;

    // ページのレイヤー番号
    private int page1Layer;
    private int page2Layer;
    private int page3Layer;

    void Start()
    {
        // 自コンポーネントの取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // ページのレイヤー番号を設定
        page1Layer = 6;
        page2Layer = 7;
        page3Layer = 8;

        // ObjectTypeによって処理を変更
        AfterChangeLayer();
    }

    /// <summary>
    /// 破られたあとの処理
    /// </summary>
    public void HitTear()
    {
        switch (objectType)
        {
            case ObjectType.GROUND:

                switch (status)
                {
                    // 最前面にする
                    case Status.SECOND:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;
                        break;

                    // １つ前面にする
                    case Status.THIRD:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                        status = Status.SECOND;
                        break;
                }

                break;
            case ObjectType.GOAL:

                switch (status)
                {
                    // 最前面のときは消去する
                    case Status.FIRST:
                        Destroy(gameObject);
                        break;

                    // 最前面にする
                    case Status.SECOND:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;
                        break;

                    // １つ前面にする
                    case Status.THIRD:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                        status = Status.SECOND;
                        break;
                }

                break;
            case ObjectType.BLOCK:

                switch (status)
                {
                    // 最前面のときは消去する
                    case Status.FIRST:
                        Destroy(gameObject);
                        break;

                    // 最前面にする
                    case Status.SECOND:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
                        status = Status.FIRST;
                        boxCollider2D.enabled = true;
                        break;

                    // １つ前面にする
                    case Status.THIRD:
                        spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                        status = Status.SECOND;
                        break;
                }

                break;
        }
    }

    /// <summary>
    /// レイヤーを切り替えたときの処理
    /// </summary>]
    public void AfterChangeLayer()
    {
        switch (objectType)
        {
            // GROUNDはページ数によって当たり判定の有無とそれに伴う表示を変更する
            case ObjectType.GROUND:

                // どのページでも色をまず戻す
                spriteRenderer.color = Color.black;

                // ページ1のとき
                if (transform.parent.gameObject.layer == page1Layer)
                {
                    status = Status.FIRST;
                }
                // ページ2のとき
                else if (transform.parent.gameObject.layer == page2Layer)
                {
                    if (transform.parent.parent.GetChild(transform.parent.GetSiblingIndex() - 1).GetComponent<PageManager>())
                    {
                        
                    }

                    status = Status.SECOND;
                    // 半透明にする
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    // 当たり判定を一時的に無くす
                    boxCollider2D.enabled = false;
                }
                // ページ3のとき
                else if (transform.parent.gameObject.layer == page3Layer)
                {
                    status = Status.SECOND;
                    // 半透明にする
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.02f);
                    // 当たり判定を一時的に無くす
                    boxCollider2D.enabled = false;
                }

                break;

            // GOALはページ数によって当たり判定の有無とそれに伴う表示を変更する
            case ObjectType.GOAL:

                // どのページでも色をまず戻す
                spriteRenderer.color = Color.white;

                // ページ1のとき
                if (transform.parent.gameObject.layer == page1Layer)
                {
                    status = Status.FIRST;
                    // 当たり判定を一時的につける
                    boxCollider2D.enabled = true;

                    foreach (GameObject goalLine in GameObject.FindGameObjectsWithTag("GoalLine")) { goalLine.GetComponent<GoalLineManager>().SetAlpha(1f); }
                }
                // ページ2のとき
                else if (transform.parent.gameObject.layer == page2Layer)
                {
                    status = Status.SECOND;
                    // 半透明にする
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    // 当たり判定を一時的に無くす
                    boxCollider2D.enabled = false;

                    foreach (GameObject goalLine in GameObject.FindGameObjectsWithTag("GoalLine")) { goalLine.GetComponent<GoalLineManager>().SetAlpha(0.2f); }
                }
                // ページ3のとき
                else if (transform.parent.gameObject.layer == page3Layer)
                {
                    status = Status.THIRD;
                    // 半透明にする
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.02f);
                    // 当たり判定を一時的に無くす
                    boxCollider2D.enabled = false;

                    foreach (GameObject goalLine in GameObject.FindGameObjectsWithTag("GoalLine")) { goalLine.GetComponent<GoalLineManager>().SetAlpha(0.02f); }
                }

                break;

            // BLOCKはページ数によって当たり判定の有無とそれに伴う表示を変更する
            case ObjectType.BLOCK:

                // どのページでも色をまず戻す
                spriteRenderer.color = layerColor[0];

                // ページ1のとき
                if (transform.parent.gameObject.layer == page1Layer)
                {
                    status = Status.FIRST;
                    // 当たり判定を一時的につける
                    boxCollider2D.enabled = true;
                }
                // ページ2のとき
                else if (transform.parent.gameObject.layer == page2Layer)
                {
                    status = Status.SECOND;
                    // 半透明にする
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.2f);
                    // 当たり判定を一時的に無くす
                    boxCollider2D.enabled = false;
                }
                // ページ3のとき
                else if (transform.parent.gameObject.layer == page3Layer)
                {
                    status = Status.THIRD;
                    // 半透明にする
                    spriteRenderer.color = new(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.02f);
                    // 当たり判定を一時的に無くす
                    boxCollider2D.enabled = false;
                }

                break;
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
    public Status GetStatus() { return status; }
}
