using UnityEngine;

public class PlayerTear : MonoBehaviour
{
    private PlayerController controller;

    private bool isActive;

    public void Initialize(PlayerController _controller)
    {
        controller = _controller;
    }

    void Update()
    {
        if (!isActive && controller.IsGrounded() && Input.GetButtonDown("Special"))
        {
            controller.SetDefault();
            isActive = true;
        }

        if (isActive)
        {
            if (Input.GetAxis("Horizontal2") < 0f)
            {
                foreach(GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    if (fieldObject.transform.position.x < Mathf.RoundToInt(transform.position.x))
                    {
                        fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                    }
                }

                isActive = false;
            }
            else if (Input.GetAxis("Horizontal2") > 0f)
            {
                foreach (GameObject fieldObject in GameObject.FindGameObjectsWithTag("FieldObject"))
                {
                    if (fieldObject.transform.position.x > Mathf.RoundToInt(transform.position.x))
                    {
                        fieldObject.GetComponent<AllFieldObjectManager>().HitTear();
                    }
                }

                isActive = false;
            }
        }
    }

    // Getter
    public bool GetIsActive() { return isActive; }
}
