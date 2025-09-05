using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class PlayerAnimationScript : MonoBehaviour
{
    private GameObject player;
    private PlayerSpriteScript spriteScript;
    private Animator animator;
    private PlayerController controller;
    private PlayerCut cut;
    private SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer playerSpriteRenderer;

    [SerializeField] private ScissorsScript scissorsPrefab;
    [SerializeField] private ScissorsScript scissors;

    [Foldout("確認")][SerializeField] private bool isCutReady = false;
    private bool isDash = false, preIsDash = false;

    float size = 1f;

    [Foldout("確認")] [SerializeField] private  int direction = 0;

    //ダッシュ
    [Foldout("ダッシュ")][SerializeField] private float dashFlowSpeed = 0;
    [Foldout("ダッシュ")][SerializeField] private float dashFlowMulti = 0.1f;
    private float dashRot = 0;

    //ハサミ
    [Foldout("ハサミ")] [SerializeField]  private Vector3 scissorsHoldOffset;
    [Foldout("ハサミ")] [SerializeField] private float scissorsMoveSpeed = 10f;
    [Foldout("ハサミ")][SerializeField] private float scissorsCutTime = 0.5f;
    [Foldout("ハサミ")][SerializeField] private float scissorsSizePlusSpeed = 10f;
    [Foldout("ハサミ")][SerializeField] private float scissorsMaxSize = 2f;
    [Foldout("確認")][SerializeField] bool isCut = false, preIsCut = false;
    float angle = 0f;
    [Foldout("調整")][SerializeField] Tween cutTween;

    [Foldout("調整")][SerializeField] Vector2 screenSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = transform.parent.gameObject;
        animator = GetComponent<Animator>();
        spriteScript = GetComponent<PlayerSpriteScript>();
        controller = player.GetComponent<PlayerController>();
        cut = player.GetComponent<PlayerCut>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

        playerSpriteRenderer.enabled = false;
        spriteRenderer.enabled = true;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPos = Camera.main.transform.position;

        preIsDash = !isDash;
        isDash = controller.GetIsRocketMoving();

        if (Input.GetButtonDown("Reset")) Init();

        if (isCutReady == true)
        {
            //カットモード入った時はカットモードのdirection優先
            direction = cut.GetDirection();
            controller.SetDirection(direction);
        }
        else
        {
            //その逆
            direction = controller.GetDirection();
            cut.SetDirection(direction);
        }

        if (isDash == true)
        {
            dashRot += Time.deltaTime * dashFlowSpeed;
            this.transform.localPosition = Vector3.up * Mathf.Sin(dashRot * Mathf.Deg2Rad) * dashFlowMulti + Vector3.right * Mathf.Cos(dashRot * Mathf.Deg2Rad) * dashFlowMulti;
        }
        else if (preIsDash == true && isDash == false)
        {
            dashRot = 0;
            this.transform.localPosition = Vector3.zero;
        }

        isCutReady = cut.GetIsActive();

        if (isCutReady == true && isCut == false)
        {
            if (scissors == null)
            {
                scissors = Instantiate(scissorsPrefab,this.transform.position,Quaternion.identity);
            }

            scissors.transform.position = Vector3.MoveTowards(
            scissors.transform.position,          // 現在位置
            this.transform.position + scissorsHoldOffset,
            scissorsMoveSpeed * Time.deltaTime       // 1フレーム分の移動距離
             );
            size = Mathf.MoveTowards(size, scissorsMaxSize, scissorsSizePlusSpeed * Time.deltaTime);
        }
        else if (isCut == true)
        {
            if (scissors != null)
            {
                //最初
                if (preIsCut == false && isCut == true)
                {
                    Vector3 pos = this.transform.position;

                    if (direction == 0) { pos.x += 0.5f; pos.y = cameraPos.y + screenSize.y * 0.5f; }
                    else if (direction == 2) { pos.x += -0.5f; pos.y = cameraPos.y + screenSize.y * 0.5f; }
                    else if (direction == 1) { pos.x = cameraPos.x + screenSize.x * -0.5f; pos.y += 0.5f; }
                    else if (direction == 3) { pos.x = cameraPos.x + screenSize.x * -0.5f; pos.y += -0.5f; }

                    scissors.transform.position = pos;

                    if (direction == 1 || direction == 3) { pos.x += screenSize.x; }
                    else if (direction == 0 || direction == 2) { pos.y += -screenSize.y; }

                    if (direction == 0 || direction == 2) angle = 0.0f;
                    else if (direction == 1 || direction == 3) angle = 90.0f;

                    cutTween = scissors.transform.DOMove(pos, scissorsCutTime).SetEase(Ease.OutCubic).OnComplete(() =>
                    {
                        preIsCut = false;
                        isCut = false;
                        angle = 0.0f;
                    });
                    spriteScript.SetScissors(false);
                }


                

                preIsCut = isCut;
                size = Mathf.MoveTowards(size, scissorsMaxSize, scissorsSizePlusSpeed * Time.deltaTime);
            }
        }
        else if (isCut == false && isCutReady == false && scissors != null)
        {

            size = Mathf.MoveTowards(size, 1f, scissorsSizePlusSpeed * Time.deltaTime);
            scissors.transform.position = Vector3.MoveTowards(
             scissors.transform.position,          // 現在位置
            this.transform.position,
            scissorsMoveSpeed * Time.deltaTime       // 1フレーム分の移動距離
            );

            if (Vector3.Distance(this.transform.position, scissors.transform.position) < 0.5f)
            {
                Destroy(scissors.gameObject);
                scissors = null;

                spriteScript.SetScissors(true);
            }

        }


        if (scissors != null)
        {
            scissors.transform.localScale = Vector3.one * size;
            scissors.transform.eulerAngles = Vector3.forward * angle;
            scissors.SetCutAnimation(isCut);
        }

        animator.SetBool("isCutReady", isCutReady);
        animator.SetBool("isDash", isDash);
        spriteScript.SetDirection(direction);

    }

    public void SetCutReady(bool cutReady_) { isCutReady = cutReady_; }
    public void SetDash(bool dash) { isDash = dash; }

    //俺はrocketをdashと呼んでる
    public  void StartRocket() { animator.SetTrigger("dash"); }
    public void StartCut()
    {
        animator.SetTrigger("cut");
        preIsCut = false;
        isCut = true;
        cutTween.Kill();
    }

    public void StartDeath()
    {
        animator.SetTrigger("death");
    }

    public void StartRespawn()
    {
        animator.SetTrigger("respawn");
    }

    private void Init()
    {
        if (scissors != null)
        {
            Destroy(scissors.gameObject);
        }
        isCutReady = false;
        size = 1f;
        angle = 0;
        spriteScript.SetDirection(0);
        preIsCut = false;
        isCut = false;
        cutTween.Kill();
    }

}
