using UnityEngine;

public class SellConnectCollider : MonoBehaviour
{
    [SerializeField] StageCell myStageCell;
    [SerializeField] bool[] direction=new bool[4];
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [ContextMenu("ê⁄ë±ÉZÉãÇçXêV")]
    public void EditorConnect()
    {

        
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position,
            Vector2.one * 0.1f,
            0f
        );

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("CellBody")) continue;

            StageCell other = hit.transform.parent.GetComponent<StageCell>();
            if (other == null) continue;

            for (int i = 0; i < direction.Length; i++)
            {
                if (direction[i])
                {
                    myStageCell.SetConnectCell(i, other);
                }
            }
        }
    }
}
