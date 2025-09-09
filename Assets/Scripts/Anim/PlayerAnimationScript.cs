using DG.Tweening;
using DG.Tweening.Core.Easing;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerAnimationScript : MonoBehaviour
{
    private GameObject player;
    private PlayerSpriteScript spriteScript;
    private PlayerManager manager;
    private Animator animator;
    private PlayerController controller;
    private PlayerCut cut;
    private SpriteRenderer spriteRenderer;
    private UndoManager undoManager;
    ParticleInstantiateScript particle;
    [SerializeField] SpriteRenderer playerSpriteRenderer;

    [SerializeField] private ScissorsScript scissorsPrefab;
    [SerializeField] private ScissorsScript scissors;

    [Foldout("確認")][SerializeField] private bool isCutReady = false;
    private bool isDash = false, preIsDash = false;
    bool isDeath = false;
    [Foldout("確認")][SerializeField] bool isHit = false, preIsHit = false;

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
    bool isDivision = false;

    [Foldout("調整")][SerializeField] Vector2 screenSize;

    Vector3 pos, prePos;
    bool isCrash = false;

    ScissorsScript deathScissors;

    [Foldout("ぶつかり")][SerializeField] private float hitMoveTime = 0.2f;

    bool isClear = false,preIsClear = false;
    [Foldout("花火")][SerializeField] private float clearSpeed = 10f;
    [Foldout("花火")][SerializeField] private float[] clearAngle = new float[2];
    bool rotated = false;
    bool isClearShot = false;
    [Foldout("花火")] [SerializeField]  private float[] clearChargeStats;

    GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = transform.parent.gameObject;
        animator = GetComponent<Animator>();
        spriteScript = GetComponent<PlayerSpriteScript>();
        controller = player.GetComponent<PlayerController>();
        manager = player.GetComponent<PlayerManager>();
        cut = player.GetComponent<PlayerCut>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        particle = GetComponent<ParticleInstantiateScript>();

        playerSpriteRenderer.enabled = false;
        spriteRenderer.enabled = true;

        undoManager = GameObject.FindGameObjectWithTag("GameController").gameObject.GetComponent<UndoManager>();

        if(cut.GetIsCreateLineStart()) spriteScript.SetScissors(false);

        gameManager=FindFirstObjectByType<GameManager>();
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Vector3 cameraPos = Camera.main.transform.position;

        preIsDash = !isDash;
        isDash = controller.GetIsRocketMoving();


        if (isDeath == false)
        {
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
        }


        if (!gameManager.GetIsGoal() && Input.GetButtonDown("Reset")) Init();
        if (!gameManager.GetIsGoal() && Input.GetButtonDown("Undo")) Init();

        if (isClear == false)
        {
            if (isHit == true)
            {
                if (preIsHit == false)
                {
                    if (direction == 0 || direction == 2)
                    {
                        float isLeftMulti = 1f;
                        if (direction == 2) isLeftMulti = -1f;

                        this.transform.DOLocalMoveY(1f, hitMoveTime * 0.5f).SetLoops(2, LoopType.Yoyo);
                        this.transform.DOLocalRotate(Vector3.forward * 360f * isLeftMulti, hitMoveTime, RotateMode.LocalAxisAdd).OnComplete(() =>
                        {
                            preIsHit = false;
                            isHit = false;
                            this.transform.localRotation = Quaternion.identity;
                            this.transform.localPosition = Vector3.zero;
                        });
                    }
                    else
                    {
                        this.transform.DOLocalMoveY(0.1f, hitMoveTime * 0.5f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
                        {
                            preIsHit = false;
                            isHit = false;
                            this.transform.localPosition = Vector3.zero;
                        });
                    }
                }

                if (isCutReady == true || isDeath == true)
                {
                    this.transform.DOComplete();
                }
                preIsHit = isHit;
            }

            if (isDash == true)
            {
                dashRot += Time.deltaTime * dashFlowSpeed;
                this.transform.localPosition = Vector3.up * Mathf.Sin(dashRot * Mathf.Deg2Rad) * dashFlowMulti + Vector3.right * Mathf.Cos(dashRot * Mathf.Deg2Rad) * dashFlowMulti;
            }
            else if (preIsDash == true && isDash == false)
            {
                dashRot = 0;
                //this.transform.localPosition = Vector3.zero;
            }

            isCutReady = cut.GetIsActive();

            if (isCutReady == true && isCut == false)
            {
                if (scissors == null)
                {
                    scissors = Instantiate(scissorsPrefab, this.transform.position, Quaternion.identity);
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

                        if(isDivision == true)
                        {
                            if (direction == 1 || direction == 3) pos.y = cut.GetDivisionPosition().y;
                            else if (direction == 0 || direction == 2) pos.x = cut.GetDivisionPosition().x;

                            isDivision = false;
                        }

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
        }
        else if (isClear)
        {
            Clear();

            if (scissors != null) {
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
        }


        animator.SetBool("isCutReady", isCutReady);
        animator.SetBool("isDash", isDash);
        spriteScript.SetDirection(direction);


    }
    public void SummonScissors()
    {
        if (scissors == null)
        {
            scissors = Instantiate(scissorsPrefab, this.transform.position, Quaternion.identity);
        }
    }
    public void SetCutReady(bool cutReady_) { isCutReady = cutReady_; }
    public void SetDash(bool dash) { isDash = dash; }

    //俺はrocketをdashと呼んでる
    public  void StartRocket() { animator.SetTrigger("dash"); }
    public void StartCut()
    {
        if (isCut == false)
        {
            SummonScissors();
            animator.SetTrigger("cut");
            preIsCut = false;
            isCut = true;
            cutTween.Kill();
        }
    }

    public void SetDivision(bool division)
    {
        isDivision = division;
    }

    public void StartDeath()
    {
        animator.SetTrigger("death");

        pos = this.transform.position;
        prePos = undoManager.GetPrevPlayerPosition();

        Vector3 distancePos = pos - prePos;


        if (Mathf.Abs(distancePos.y) > Mathf.Abs(distancePos.x) && distancePos.y < 0) direction = 3;

        float deathTime = manager.GetDeathTime();

        deathScissors = Instantiate(scissorsPrefab, this.transform.position, Quaternion.identity);
        deathScissors.transform.DORotate(Vector3.forward * 720f,deathTime,RotateMode.LocalAxisAdd);
        deathScissors.transform.DOMove(prePos, deathTime).SetEase(Ease.Linear).OnComplete(() =>
        {
                Destroy(deathScissors.gameObject);
        });

        isDeath = true;
    }

    public void StartRespawn()
    {
        if (deathScissors != null) Destroy(deathScissors.gameObject);
        animator.SetTrigger("respawn");
        isDeath = false;
    }

    public void StartHit()
    {
        animator.SetTrigger("dashHit");
        isHit = true;
    }

    private void Init()
    {
       // if(deathScissors != null) Destroy(deathScissors.gameObject);

        if (scissors != null)
        {
            Destroy(scissors.gameObject);
        }
        clearAngle[0] = 0;
        rotated = false;
        isClearShot = false;
        isCutReady = false;
        size = 1f;
        angle = 0;
        spriteScript.SetDirection(0);
        preIsCut = false;
        isCut = false;
        cutTween.Kill();
        this.transform.localRotation = Quaternion.identity;
        this.transform.localPosition = Vector3.zero;
        preIsHit = false;
        isHit = false;
    }

    [Button]
    public void StartClear()
    {
        clearAngle[0] = 0;
        rotated = false;
        isClearShot = false;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localPosition = Vector3.zero;
        isClear = true;
        animator.SetTrigger("clear");
    }

    public void CrashCut()
    {
        if (isCrash == true)
        {
            SetDivision(true);
            StartCut();
            isCrash = false;
        }
    }

    public void SetCrash(bool crash_)
    {
        isCrash = true;
    }

    public void Punch()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 2, 1f);
    }
    private void Clear()
    {
        float isLeftMulti = 1f;
        if (direction == 2f)
        {
            isLeftMulti = -1f;

        }

        float plusAngle = 0f;
        if (direction == 1f) plusAngle = 90f;
        else if (direction == 3f) plusAngle = -90f;

        if (isClearShot == false)
        {
            Vector3 rand = Vector3.zero;
            rand.x = Random.Range( -clearChargeStats[0], clearChargeStats[0]);
            rand.y = Random.Range(-clearChargeStats[0], clearChargeStats[0]);

            if (clearChargeStats[2] > clearChargeStats[1])
            {
                isClearShot = true;
                Vector3 particlePos = new Vector3(-0.56f * isLeftMulti, -0.18f,0f);
                Vector3 scale = new Vector3(isLeftMulti,1f,1f);
                int particleNum = 0;

                if (direction == 1)
                {
                    particlePos = new Vector3(-0.18f, -0.54f, 0f);
                    particleNum = 1;
                }
                else if (direction == 3)
                {
                    particlePos = new Vector3(-0.18f, 0.54f, 0f);
                    scale = new Vector3(1f, -1f, 1f);
                    particleNum = 1;
                }
                particle.RunParticleChild(particleNum, particlePos + this.transform.position,scale);
                Punch();
                this.transform.localRotation = Quaternion.identity;
                this.transform.localPosition = Vector3.zero;
            }
            else
            {
                clearChargeStats[2] += Time.deltaTime;
            }

            this.transform.localPosition = rand;
        }
        else
        {
            if (clearAngle[0] < 15f && rotated == false)
            {
                clearAngle[0] += clearAngle[1] * Time.deltaTime;
            }
            else if (clearAngle[0] < 370f && rotated == false)
            {
                clearAngle[0] += clearAngle[2] * Time.deltaTime;
            }
            else
            {
                if (rotated == false)
                {
                    rotated = true;
                    clearAngle[0] = 370f;
                }
                clearAngle[0] += clearAngle[3] * Time.deltaTime ;
            }

            float rad = Mathf.Deg2Rad * (clearAngle[0] + plusAngle);

            Vector3 velocity = Vector3.zero;

            velocity.x = Mathf.Cos(rad) * clearSpeed;
            velocity.y = Mathf.Sin(rad) * clearSpeed* isLeftMulti;

            this.transform.localPosition += velocity * Time.deltaTime * isLeftMulti;
            this.transform.eulerAngles = Vector3.forward * clearAngle[0] * isLeftMulti;
        }
    }

}
