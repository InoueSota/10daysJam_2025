using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageVignetteScript : MonoBehaviour
{
    public Volume volume; // ビネットが含まれている Volume をインスペクターから設定
    Vignette vignette;
    float vinetteIntesity;
   
    public void SetVinetteIntesity(float intesity)
    {
        vinetteIntesity=intesity;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (volume != null && volume.profile.TryGet(out vignette))
        {
            Debug.Log("Vignette found");
        }
        else
        {
            Debug.LogWarning("Vignette not found in Volume profile");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (vignette == null) { return; }
        if (vinetteIntesity > 0)
        {
            vinetteIntesity -= Time.unscaledDeltaTime;
            vignette.intensity.Override(Mathf.Clamp01(vinetteIntesity));
        }
    }
}
