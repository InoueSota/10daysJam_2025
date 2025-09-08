using UnityEngine;

public class AreaDeath : MonoBehaviour
{
    [SerializeField] Vector3 targetPos;
    [SerializeField] Transform area;
    [SerializeField] Transform deadLine;
    [SerializeField] bool isleft;
    [SerializeField] Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //ディヴィジョンラインが縦プレイヤーがディヴィジョンライン左だったら
        if (player.position.x < deadLine.position.x&&player.FindChild("DivisionLine").GetComponent<DivisionLineManager>().GetDivisionMode()==DivisionLineManager.DivisionMode.VERTICAL)
        {
            area.
        }
        else
        {

        }
        //ディヴィジョンラインが横プレイヤーがディヴィジョンライン下だったら
        if (player.position.y < deadLine.position.y && player.FindChild("DivisionLine").GetComponent<DivisionLineManager>().GetDivisionMode() == DivisionLineManager.DivisionMode.HORIZONTAL)
        {

        }
        else
        {

        }
    }
}
