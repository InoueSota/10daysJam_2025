using UnityEditor;
using UnityEngine;

public class SetMouceCursorPosition : MonoBehaviour
{
    public Camera mainCamera;       // 使用するカメラ（未設定なら自動取得）
    [SerializeField] float circleRadius;
    [SerializeField] Transform circleObj;
    [SerializeField] float zPosition;
    [SerializeField] float deadZone;

    public Vector2 GetControllerInput()
    {
        Vector2 moucePos = transform.position - circleObj.position;

        if (moucePos.magnitude < deadZone) { return Vector2.zero; }
        Vector2 ratio = moucePos / circleRadius;
        return ratio;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        circleRadius *= transform.parent.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {


        Vector2 newPos = GetMouseWorldPosition();
        Vector2 circlePos = circleObj.position;
        Vector2 direction = newPos - circlePos;

        if (direction.sqrMagnitude > circleRadius * circleRadius) // 半径を超えたら
        {
            newPos = circlePos + direction.normalized * circleRadius;

        }
        this.transform.position = new Vector3(newPos.x, newPos.y, zPosition);
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        return mainCamera.ScreenToWorldPoint(mousePosition);
    }
}
