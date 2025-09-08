using UnityEngine;

public class AreaDeath : MonoBehaviour
{
    [SerializeField] Vector3 localPos;
    [SerializeField] Vector3 localScale;
    [SerializeField] Transform area;
    [SerializeField] Transform deadLine;
    [SerializeField] bool isleft;
    [SerializeField] Transform player;

    void Start()
    {
        //プレイヤーの情報を取得
        player = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
        //ポジションを保持しておく
        localPos = area.localPosition;
        localScale = area.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        //�f�B���B�W�������C�����c�v���C���[���f�B���B�W�������C������������
        if (GameObject.FindGameObjectWithTag("DivisionLine").GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.VERTICAL)
        {
            if (player.position.x < deadLine.position.x)
            {
                Debug.Log("左にいるよ");
                area.localPosition = new Vector3(localPos.x, 0, 0);
            }
            else
            {
                area.localPosition = new Vector3(localPos.x - localScale.x, 0, 0);
                Debug.Log("→にいるよ");

            }
        }
        if (GameObject.FindGameObjectWithTag("DivisionLine").GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
        {
            if (player.position.y < deadLine.position.y)
            {
                Debug.Log("下にいるよ");
                area.localPosition = new Vector3(localPos.x, 0, 0);
            }
            else
            {
                area.localPosition = new Vector3(localPos.x - localScale.x, 0, 0);
                Debug.Log("↑にいるよ");

            }
        }
    }
}
