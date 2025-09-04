using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeScript : MonoBehaviour
{
    public float shakeTime;
    float shakeTimeCount;
    public float shakeNum;
    float shakeNumCount;
    //public float shakeSize;
    //float currentShakeSize;
    [SerializeField] Vector3 shakeSize;
    Vector3 currentShakeSize;
    Vector3 newPosition;
    Vector3 initPos;
    public bool isShake;
    public bool isStartResetUpdate = false; // start‚µ‚½Žž‚É’l‚ðreset‚·‚é‚©‚Ç‚¤‚©
    public bool loopShake;
    RectTransform rectTransform;
    Transform transformPos;
    // Start is called before the first frame update
    void Start()
    {
        shakeNumCount = shakeNum;
        shakeTimeCount = 0;
        transformPos = GetComponent<Transform>();
        initPos = transformPos.localPosition;
    }
    public bool GetisShake() { return isShake; }
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G))
        {
            ShakeStart();
        }
#endif
        ShakeUpdate();
        LoopShake();
    }

    public void SetShakeParamater()
    {
        shakeNumCount = shakeNum;
        currentShakeSize = shakeSize;
    }
    public void ShakeStart()
    {
        if (!isShake)
        {
            isShake = true;
            initPos = transformPos.localPosition;
        }
        else
        {
            if (!isStartResetUpdate)
            {
                SetShakeParamater();
            }
        }

    }

    void ShakeUpdate()
    {
        if (loopShake)
        {
            return;
        }
        if (!isShake) { return; }
        if (shakeTimeCount > 0)
        {
            shakeTimeCount -= Time.deltaTime;
            return;
        }
        shakeTimeCount = shakeTime;
        shakeNumCount--;

        // Debug.Log("ShakeNum=" + shakeNumCount);
        //shakeI‚í‚è
        if (shakeNumCount == 0)
        {
            isShake = false;
            shakeNumCount = shakeNum;
            // rectTransform.anchoredPosition = Vector3.zero;
            transformPos.localPosition = initPos;
            shakeTimeCount = 0;
            return;
        }
        if (shakeSize == Vector3.zero) {
            Debug.LogError(transform.name + "‚ÌShakeSize‚ª0‚Å‚·");
            return; }
        currentShakeSize = shakeSize * (shakeNumCount / shakeNum);
        newPosition = initPos;
        if (currentShakeSize.x != 0)
        {
            newPosition.x += Random.Range(-currentShakeSize.x, currentShakeSize.x);

        }
        if (currentShakeSize.y != 0)
        {
            newPosition.y += Random.Range(-currentShakeSize.y, currentShakeSize.y);
        }
        if (currentShakeSize.z != 0)
        {
            newPosition.z += Random.Range(-currentShakeSize.z, currentShakeSize.z);
        }
        transformPos.localPosition = newPosition;

    }

    void LoopShake()
    {
        if (!loopShake)
        {
            return;
        }
        if (!isShake) { 
            
            return;
        
        }

        if (shakeTimeCount > 0)
        {
            shakeTimeCount -= Time.deltaTime;
            return;
        }
        shakeTimeCount = shakeTime;
        newPosition = initPos;
        currentShakeSize = shakeSize;
        if (currentShakeSize.x != 0)
        {
            newPosition.x += Random.Range(-currentShakeSize.x, currentShakeSize.x);

        }
        if (currentShakeSize.y != 0)
        {
            newPosition.y += Random.Range(-currentShakeSize.y, currentShakeSize.y);
        }
        if (currentShakeSize.z != 0)
        {
            newPosition.z += Random.Range(-currentShakeSize.z, currentShakeSize.z);
        }
        transformPos.localPosition = newPosition;
    }
}
