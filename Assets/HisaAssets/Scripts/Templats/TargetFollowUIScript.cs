using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollowUIScript : MonoBehaviour
{

    [SerializeField] RectTransform target;
    RectTransform thisRectTransform;
    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("target‚ªnull‚Å‚·", this.gameObject);
        }
        thisRectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        thisRectTransform.anchoredPosition = target.anchoredPosition;
    }
}
