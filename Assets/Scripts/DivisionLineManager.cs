using UnityEngine;

public class DivisionLineManager : MonoBehaviour
{
    public enum DivisionMode
    {
        HORIZONTAL,
        VERTICAL
    }
    private DivisionMode divisionMode;

    public void Initialize(DivisionMode _divisionMode) { divisionMode = _divisionMode; }

    public DivisionMode GetDivisionMode() { return divisionMode; }
}
