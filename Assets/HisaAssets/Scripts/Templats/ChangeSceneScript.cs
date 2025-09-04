using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneScript : MonoBehaviour
{
    bool isFadeOut;
    public FadeScript fadaeObjPrefab;
    [SerializeField, Header("ëJà⁄êÊÉVÅ[Éìñº")] string seneName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isFadeOut)
            {
                isFadeOut = true;
                FadeScript fadeObj = Instantiate(fadaeObjPrefab);
                fadeObj.SetSceneName(seneName);

            }
        }
    }
}
