using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    // プレイヤー関係
    public Vector3 playerPosition;
    public Vector2 divisionPosition;
    public bool isDivision;
    public bool isMoving;
    public GameObject warpObj;

    // ブロックの親関係
    public Vector3 objectParentPosition1;
    public Vector3 objectParentPosition2;

    // ブロック関係
    public List<Vector3> blockPositions = new List<Vector3>();
    public List<Vector3> blockPrePositions = new List<Vector3>();
    public List<Vector3> blockCurrentPositions = new List<Vector3>();
    public List<Transform> blockParents = new List<Transform>();
    public List<bool> blockActiveStates = new List<bool>();

    // 分断線関係
    public Vector3 divisionLinePosition;
    public Quaternion divisionLineRotation;
    public bool divisionLineActiveState;
}
