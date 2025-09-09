using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CrabSpriteScript : MonoBehaviour
{
    [SerializeField] SpriteRenderer mainSprite;
    SpriteRenderer sprite;
    Animator animator;
    [SerializeField] float throwTime = 0.5f;

    [SerializeField] CrabManager crab;

    [SerializeField] bool isThrow = false;
    [SerializeField] int firstDirection = 2;

    DivisionLineManager divisionLineManager;
    PlayerCut cut;

    bool isSleep = false, preIsSleep = false;

    [SerializeField] ParticleSystem sleepParticle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cut = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerCut>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        mainSprite.enabled = false;
        sprite.enabled = true;

        animator.SetTrigger("directionChange");
        animator.SetInteger("direction", firstDirection);
        divisionLineManager = GameObject.FindGameObjectWithTag("DivisionLine").GetComponent<DivisionLineManager>();
        sleepParticle.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetInteger("direction", (int)crab.GetThrowDirection());

        if (Input.GetButtonDown("Reset")) Init();
        if (Input.GetButtonDown("Undo")) Init();

        SleepCheck();
    }

    public void StartTheow()
    {
        if (isThrow == false)
        {
            transform.DOLocalMove(crab.GetThrowVector(), throwTime * 0.5f).SetLoops(2,LoopType.Yoyo).OnComplete(() =>
            {
                isThrow = false;
            });
            isThrow = true;
        }
    }

    private void Init()
    {
        isThrow = false ;
        transform.localPosition = Vector3.zero;
        transform.DOComplete();
    }

    public void ChangeDirection(int direction)
    {
        animator.SetInteger("direction", direction);
        animator.SetTrigger("directionChange");

        if(direction == 2) sprite.flipX = false; 
       else if (direction == 0) sprite.flipX = true;
        if (direction == 3) sprite.flipY = true;
        else if (direction == 1) sprite.flipY = false;
    }

    private void SleepCheck()
    {
        preIsSleep = isSleep;
        isSleep = false;
        if (cut.GetIsDivision() == true)
        {
            if (divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
            {
                if (crab.transform.position.x < divisionLineManager.transform.position.x
                    && cut.transform.position.x > divisionLineManager.transform.position.x)
                {
                    isSleep = true;
                }
                else if (crab.transform.position.x > divisionLineManager.transform.position.x
                    && cut.transform.position.x < divisionLineManager.transform.position.x)
                {
                    isSleep = true;
                }
            }
           else if (divisionLineManager.GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
            {
                if (crab.transform.position.y < divisionLineManager.transform.position.y &&
                    cut.transform.position.y > divisionLineManager.transform.position.y)
                {
                    isSleep = true;
                }
                else if (crab.transform.position.y > divisionLineManager.transform.position.y &&
                    cut.transform.position.y < divisionLineManager.transform.position.y)
                {
                    isSleep = true;
                }
            }
        }
        animator.SetBool("isSleep", isSleep);
        if(isSleep == true && preIsSleep == false)
        {
            sleepParticle.Play();
        }
        else if (isSleep == false && preIsSleep == true)
        {
            sleepParticle.Stop();
            animator.SetTrigger("directionChange");
        }
    }
}
