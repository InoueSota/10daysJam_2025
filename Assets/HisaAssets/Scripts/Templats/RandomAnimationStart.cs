using UnityEngine;

public class RandomAnimationStart : MonoBehaviour
{
    public Animator animator;
    public string animationStateName;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            float randomStartTime = Random.Range(0f, 1f);
            animator.Play(animationStateName, 0, randomStartTime);
        }
    }

    public void SetAnime()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            float randomStartTime = Random.Range(0f, 1f);
            animator.Play(animationStateName, 0, randomStartTime);
        }
    }
}
