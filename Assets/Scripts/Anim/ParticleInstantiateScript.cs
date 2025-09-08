using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleInstantiateScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] ParticleSystem[] particle;
    Transform particleParent;

    void Awake()
    {
        GameObject existing = GameObject.FindWithTag("ParticleParent");

        if (existing != null)
        {
            particleParent = existing.transform;
        }
        else
        {
            GameObject newObj = new GameObject("ParticleParent");
            newObj.tag = "ParticleParent"; // タグを設定（※あらかじめタグを作っておく必要あり）
            particleParent = newObj.transform;
        }
        //if (UnityEditorInternal.InternalEditorUtility.tags.Contains("ParticleParent"))
        //{
            
        //}
        //else
        //{
        //    Debug.LogWarning("ParticleParentタグを追加しないと、ビルドできないけど、それでいいの？");
        //}
    }

    public void RunParticle(int particleNum)
    {
        if (particle[particleNum] != null)
        {
            ParticleSystem particleObject = Instantiate(particle[particleNum], this.transform.position, Quaternion.identity);
            if (particleParent != null) {
                particleObject.gameObject.transform.parent = particleParent;
            }
        }
    }

    public void RunParticle(int particleNum, Vector3 particlePos)
    {
        if (particle[particleNum] != null)
        {
            ParticleSystem particleObject = Instantiate(particle[particleNum], particlePos, Quaternion.identity);
            if (particleParent != null)
            {
                particleObject.gameObject.transform.parent = particleParent;
            }
        }
    }

    public void RunParticle(int particleNum, Vector3 particlePos, float rot)
    {
        if (particle[particleNum] != null)
        {
            ParticleSystem particleObject = Instantiate(particle[particleNum], particlePos, Quaternion.Euler(0, 0, rot));
            if (particleParent != null)
            {
                particleObject.gameObject.transform.parent = particleParent;
            }
        }
    }

    public void RunParticle(int particleNum, Vector3 particlePos, Vector3 rot)
    {
        if (particle[particleNum] != null)
        {
            ParticleSystem particleObject = Instantiate(particle[particleNum], particlePos, Quaternion.Euler(rot.x, rot.y, rot.z));
            if (particleParent != null)
            {
                particleObject.gameObject.transform.parent = particleParent;
            }
        }
    }

    public void RunParticleChild(int particleNum, Vector3 particlePos)
    {
        if (particle[particleNum] != null)
        {
            ParticleSystem particleObject = Instantiate(particle[particleNum], particlePos, Quaternion.identity);
            particleObject.gameObject.transform.parent = this.transform;
        }
    }

    public void RunParticleChild(int particleNum, Vector3 particlePos,Vector3 scale)
    {
        if (particle[particleNum] != null)
        {
            ParticleSystem particleObject = Instantiate(particle[particleNum], particlePos, Quaternion.identity);
            particleObject.transform.localScale = scale;
            particleObject.gameObject.transform.parent = this.transform;
        }
    }


}
