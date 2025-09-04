using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    // ƒvƒŒƒCƒ„[ŠÖŒW
    public Vector3 playerPosition;
    public Vector2 divisionPosition;
    public bool isDivision;
    public bool isMoving;

    // ƒuƒƒbƒNŠÖŒW
    public List<Vector3> blockPositions = new List<Vector3>();
    public List<Vector3> blockPrePositions = new List<Vector3>();
    public List<Vector3> blockCurrentPositions = new List<Vector3>();
    public List<Vector3> blockPreRocketVector = new List<Vector3>();
    public List<bool> blockActiveStates = new List<bool>();
    public List<Transform> blockParents = new List<Transform>();

    // •ª’füŠÖŒW
    public Vector3 divisionLinePosition;
    public Quaternion divisionLineRotation;
    public bool divisionLineActiveState;
}
