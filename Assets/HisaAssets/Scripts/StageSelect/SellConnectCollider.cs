using UnityEngine;

public class SellConnectCollider : MonoBehaviour
{
    [SerializeField] StageCell myStageCell;
    [SerializeField] bool[] direction=new bool[4];
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "CellBody")
        {

            for (int i = 0; i < direction.Length; i++) {
                if (direction[i])
                {
                    myStageCell.SetConnectCell(i, collision.transform.parent.GetComponent<StageCell>());
                    //collision.transform.parent.GetComponent<StageCell>().SetConnectSell
    
                }
            }
            
        }
    }
}
