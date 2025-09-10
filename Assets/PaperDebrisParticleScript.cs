using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PaperDebrisParticleScript : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;

    [SerializeField] Vector2 screenSize = Vector3.zero;
    [SerializeField] Vector2 midPos = Vector2.zero;

    [SerializeField] Color[] areaColors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Set(Vector3 divisionPos,bool isHorizontal,string areaName)
    {
        float scale = 1f;
        float rotate = 0f;
        Vector3 pos = divisionPos;

        Color color = Color.white;
        if(areaName == "Area1") color = areaColors[0];
        else if (areaName == "Area2") color = areaColors[1];
        else if (areaName == "Area3") color = areaColors[2];
        else if (areaName == "Area4") color = areaColors[3];
        else if (areaName == "Area5") color = areaColors[4];

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
        MainModule main = particle.main;

        main.startColor = color;

        shape.rotation = Vector3.forward * rotate;
        shape.radius = scale * 0.5f;

        particle.Play();
    }
}
