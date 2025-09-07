using UnityEngine;

public class ArrowInputUI : MonoBehaviour
{

    [SerializeField] PushButton[] arrowButtons;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputDire;
        inputDire.x = Input.GetAxisRaw("Horizontal");
        inputDire.y = Input.GetAxisRaw("Vertical");

        if (inputDire.y == 0)
        {
            arrowButtons[0].Release();
            arrowButtons[1].Release();
        }else if(inputDire.y > 0)
        {
            arrowButtons[0].Push();
            arrowButtons[1].Release();

        }
        else if (inputDire.y < 0)
        {
            arrowButtons[0].Release();
            arrowButtons[1].Push();

        }

        if (inputDire.x == 0)
        {
            arrowButtons[2].Release();
            arrowButtons[3].Release();
        }
        else if (inputDire.x > 0)
        {
            arrowButtons[2].Push();
            arrowButtons[3].Release();

        }
        else if (inputDire.x < 0)
        {
            arrowButtons[2].Release();
            arrowButtons[3].Push();

        }
    }
}
