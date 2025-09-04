using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    private PlayerSpriteScript spriteScript;
    private Animator animator;
    private PlayerController controller;
    private PlayerCut cut;

    private bool isCutReady = false;
    private bool isDash = false;

   [SerializeField] private  int direction = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteScript = GetComponent<PlayerSpriteScript>();
        controller = GetComponent<PlayerController>();
        cut = GetComponent<PlayerCut>();
    }

    // Update is called once per frame
    void Update()
    {
        isDash = controller.GetIsRocketMoving();

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

        isCutReady = cut.GetIsActive();

        animator.SetBool("isCutReady", isCutReady);
        animator.SetBool("isDash", isDash);
        spriteScript.SetDirection(direction);

    }

    public void SetCutReady(bool cutReady_) { isCutReady = cutReady_; }
    public void SetDash(bool dash) { isDash = dash; }

    //俺はrocketをdashと呼んでる
    public  void StartRocket() { animator.SetTrigger("dash"); }
    public void StartCut() { animator.SetTrigger("cut"); }

}
