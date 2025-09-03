using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{
    // äYìñÇ∑ÇÈObjectType
    public enum ObjectType
    {
        GROUND,
        GOAL,
        BLOCK
    }
    [SerializeField] private ObjectType objectType;

    void Start()
    {

    }

    /// <summary>
    /// îjÇÁÇÍÇΩÇ†Ç∆ÇÃèàóù
    /// </summary>
    public void HitTear()
    {
        switch (objectType)
        {
            case ObjectType.GROUND:



                break;
            case ObjectType.GOAL:



                break;
            case ObjectType.BLOCK:



                break;
        }
    }

    // Getter
    public ObjectType GetObjectType() { return objectType; }
}
