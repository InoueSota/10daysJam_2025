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

        isCutReady = cut.GetIsActive();

        animator.SetBool("isCutReady", isCutReady);
        animator.SetBool("isDash", isDash);
        spriteScript.SetDirection(direction);

    }

    public void SetCutReady(bool cutReady_) { isCutReady = cutReady_; }
    public void SetDash(bool dash) { isDash = dash; }

    //����rocket��dash�ƌĂ�ł�
    public  void StartRocket() { animator.SetTrigger("dash"); }
    public void StartCut() { animator.SetTrigger("cut"); }

}
