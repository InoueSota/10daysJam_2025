using UnityEngine;

public class ScissorsScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetCutAnimation(bool isCut)
    {
        animator.SetBool("isCut", isCut);
    }
}
