using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGameSpeed : MonoBehaviour
{
    [Header("Tを押すとdebugTimeScaleの時間になる")]
    [SerializeField,Range(1,20)] private float debugTimeScale = 1f; // デバッグ時の時間スケール
    private float originalTimeScale;
    float preTimeScale;

    private void Awake()
    {
#if UNITY_EDITOR
        originalTimeScale = Time.timeScale;
        Time.timeScale = debugTimeScale;
        preTimeScale=debugTimeScale;
#endif

    }

    private void Update()
    {
#if UNITY_EDITOR
        if (preTimeScale!=debugTimeScale)
        {
            Time.timeScale = debugTimeScale;
            //Time.timeScale = (Time.timeScale == originalTimeScale) ? debugTimeScale : originalTimeScale;
            Debug.Log("Time Scale: " + Time.timeScale);
            preTimeScale = debugTimeScale;
        }
        
#endif
    }
}
