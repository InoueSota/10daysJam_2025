using UnityEngine;

public class AllFieldObjectManager : MonoBehaviour
{
    // �Y������ObjectType
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
    /// �j��ꂽ���Ƃ̏���
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
