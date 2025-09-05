using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    private ScissorsScript scissors;

    private bool isCutReady = false;
    private bool isDash = false, preIsDash = false;


    [Foldout("�m�F")] [SerializeField] private  int direction = 0;

    //�_�b�V��
    [Foldout("�_�b�V��")][SerializeField] private float dashFlowSpeed = 0;
    [Foldout("�_�b�V��")][SerializeField] private float dashFlowMulti = 0.1f;
    private float dashRot = 0;

    //�n�T�~
    [Foldout("�n�T�~")] [SerializeField]  private Vector3 scissorsHoldOffset;
    [Foldout("�n�T�~")] [SerializeField] private float scissorsMoveSpeed = 10f;
    [Foldout("�n�T�~")][SerializeField] private float scissorsCutTime = 0.5f;
    bool isCut = false, preIsCut = false;

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
                spriteScript.SetScissors(false);
                scissors = Instantiate(scissorsPrefab,this.transform.position,Quaternion.identity);
            }

            scissors.transform.position = Vector3.MoveTowards(
            scissors.transform.position,          // ���݈ʒu
            this.transform.position + scissorsHoldOffset,
            scissorsMoveSpeed * Time.deltaTime       // 1�t���[�����̈ړ�����
        );
        }
        else if (isCut == true)
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

                scissors.transform.DOMove(pos, scissorsCutTime).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    preIsCut = false;
                    isCut = false;
                });

            }


            preIsCut = isCut;
        }
        else if (isCut == false && isCutReady == false && scissors != null)
        {
            scissors.transform.position = Vector3.MoveTowards(
             scissors.transform.position,          // ���݈ʒu
            this.transform.position,
            scissorsMoveSpeed * Time.deltaTime       // 1�t���[�����̈ړ�����
            );

            if (Vector3.Distance(this.transform.position, scissors.transform.position) < 0.5f)
            {
                spriteScript.SetScissors(true);
                Destroy(scissors.gameObject);
            }
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
        isCut = true;
    }


    }
