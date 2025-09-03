using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    // �v���C���[�֌W
    public Vector3 playerPosition;
    public Vector2 divisionPosition;
    public bool isDivision;

    // �u���b�N�֌W
    public List<Vector3> blockPositions = new List<Vector3>();
    public List<bool> blockActiveStates = new List<bool>();
    public List<Transform> blockParents = new List<Transform>();

    // ���f���֌W
    public Vector3 divisionLinePosition;
    public bool divisionLineActiveState;
}
