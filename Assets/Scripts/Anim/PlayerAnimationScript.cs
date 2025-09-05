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

    [Foldout("�m�F")][SerializeField] private bool isCutReady = false;
    private bool isDash = false, preIsDash = false;

    float size = 1f;

    [Foldout("�m�F")] [SerializeField] private  int direction = 0;

    //�_�b�V��
    [Foldout("�_�b�V��")][SerializeField] private float dashFlowSpeed = 0;
    [Foldout("�_�b�V��")][SerializeField] private float dashFlowMulti = 0.1f;
    private float dashRot = 0;

    //�n�T�~
    [Foldout("�n�T�~")] [SerializeField]  private Vector3 scissorsHoldOffset;
    [Foldout("�n�T�~")] [SerializeField] private float scissorsMoveSpeed = 10f;
    [Foldout("�n�T�~")][SerializeField] private float scissorsCutTime = 0.5f;
    [Foldout("�n�T�~")][SerializeField] private float scissorsSizePlusSpeed = 10f;
    [Foldout("�n�T�~")][SerializeField] private float scissorsMaxSize = 2f;
    [Foldout("�m�F")][SerializeField] bool isCut = false, preIsCut = false;
    float angle = 0f;
    [Foldout("����")][SerializeField] Tween cutTween;

    [Foldout("����")][SerializeField] Vector2 screenSize;

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
            //�J�b�g���[�h���������̓J�b�g���[�h��direction�D��
            direction = cut.GetDirection();
            controller.SetDirection(direction);
        }
        else
        {
            //���̋t
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
            scissors.transform.position,          // ���݈ʒu
            this.transform.position + scissorsHoldOffset,
            scissorsMoveSpeed * Time.deltaTime       // 1�t���[�����̈ړ�����
             );
            size = Mathf.MoveTowards(size, scissorsMaxSize, scissorsSizePlusSpeed * Time.deltaTime);
        }
        else if (isCut == true)
        {
            if (scissors != null)
            {
                //�ŏ�
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
             scissors.transform.position,          // ���݈ʒu
            this.transform.position,
            scissorsMoveSpeed * Time.deltaTime       // 1�t���[�����̈ړ�����
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

    //����rocket��dash�ƌĂ�ł�
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
