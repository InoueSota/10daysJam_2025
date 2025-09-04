using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGameSpeed : MonoBehaviour
{
    [Header("T��������debugTimeScale�̎��ԂɂȂ�")]
    [SerializeField,Range(1,20)] private float debugTimeScale = 1f; // �f�o�b�O���̎��ԃX�P�[��
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
