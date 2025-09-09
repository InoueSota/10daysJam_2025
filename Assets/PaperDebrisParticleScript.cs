using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PaperDebrisParticleScript : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;

    [SerializeField] Vector2 screenSize = Vector3.zero;
    [SerializeField] Vector2 midPos = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Set(Vector3 divisionPos,bool isHorizontal)
    {
        float scale = 1f;
        float rotate = 0f;
        Vector3 pos = divisionPos;

        if (isHorizontal == true)
        {
            scale = screenSize.x;
            pos.x = midPos.x;
        } else {
            scale = screenSize.y;
           pos.y = midPos.y;
            rotate = 90f;
        }

        this.transform.position = pos;
        ShapeModule shape = particle.shape;

        shape.rotation = Vector3.forward * rotate;
        shape.radius = scale * 0.5f;

        particle.Play();
    }
}
