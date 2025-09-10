using UnityEngine;
public class TitleLogoScript : MonoBehaviour
{

    [SerializeField] GameObject normalLogo;
    [SerializeField] float normalLogoMoveSpeed;
    [SerializeField] float normalLogoMoveRange;

    [SerializeField] TitileManager titleManager;

    [SerializeField] Canvas titleName;
    [SerializeField] Canvas titleCanvas;
    [SerializeField] GameObject[] hideObjects;

    Animator animator;

    float angle = 0f;

    int logoMoveFlag = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (logoMoveFlag == 0)
        {
            NormalLogoMove();
            if (Input.GetButtonDown("Select"))
            {
                SetFlag1();
            }
        }
    }

    void NormalLogoMove()
    {
        angle += normalLogoMoveSpeed * Time.deltaTime;

        if (angle > 360f) angle -= 360f;

        Vector3 pos = Vector3.zero;
        float rad = Mathf.Deg2Rad * angle;

        //pos.x = Mathf.Cos(rad);
        pos.y = Mathf.Sin(rad);

        normalLogo.transform.localPosition = pos * normalLogoMoveRange;
    }

    void SetFlag1()
    {
        animator.SetTrigger("Start");
        for (int i = 0; i < hideObjects.Length; i++)
        {
            hideObjects[0].SetActive(false);
        }
        normalLogo.transform.localPosition = Vector3.zero;
        titleCanvas.enabled = false;
        titleName.enabled = false;
        logoMoveFlag = 1;
    }

    public void SetFlag2()
    {
        titleManager.SetStart();
        logoMoveFlag = 2;
    }
}
