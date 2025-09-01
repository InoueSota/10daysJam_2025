using UnityEngine;

public class AllBlockManager : MonoBehaviour
{
    void Start()
    {
        if (transform.parent.gameObject.layer == 6)
        {
            GetComponent<SpriteRenderer>().color = Color.black;
        }
        else if (transform.parent.gameObject.layer == 7)
        {
            GetComponent<SpriteRenderer>().color = new(0f, 0f, 0f, 0.5f);
        }
    }

    void Update()
    {
        
    }
}
