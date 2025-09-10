using UnityEngine;
using UnityEngine.Tilemaps;

public class TitleDirection : MonoBehaviour
{
    enum Status
    {
        AREA1, AREA2, AREA3, AREA4, AREA5
    }
    private Status status = Status.AREA1;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Color[] backgroundColors;
    private Color targetBackground;

    [Header("Area")]
    [SerializeField] private Tilemap[] areaTilemaps;
    [SerializeField] private SpriteRenderer[] areabgRenderers;
    [SerializeField] private float chasePower;
    private Color[] targetColor = new Color[5];

    [Header("Sun")]
    [SerializeField] private SpriteRenderer sunRenderer;

    [Header("Interval")]
    [SerializeField] private float intervalTime;
    private float intervalTimer;

    void Start()
    {
        targetBackground = backgroundColors[0];
        mainCamera.backgroundColor = backgroundColors[0];

        for (int i = 0; i < 5; i++)
        {
            targetColor[i] = new(1f, 1f, 1f, 0f);
            areaTilemaps[i].color = areaTilemaps[i].color + (targetColor[i] - areaTilemaps[i].color) * (chasePower * Time.deltaTime);
            areabgRenderers[i].color = areabgRenderers[i].color + (targetColor[i] - areabgRenderers[i].color) * (chasePower * Time.deltaTime);
        }
        sunRenderer.color = targetColor[0];

        targetColor[0] = new(1f, 1f, 1f, 1f);
        areaTilemaps[0].color = targetColor[0];
        areabgRenderers[0].color = targetColor[0];
        status = Status.AREA1;

        intervalTimer = intervalTime;
    }

    void Update()
    {
        intervalTimer -= Time.deltaTime;

        if (intervalTimer < 0f)
        {
            targetColor[(int)status] = new(1f, 1f, 1f, 0f);
            switch (status)
            {
                case Status.AREA1:
                    targetColor[1] = new(1f, 1f, 1f, 1f);
                    status = Status.AREA2;
                    break;
                case Status.AREA2:
                    targetColor[2] = new(1f, 1f, 1f, 1f);
                    status = Status.AREA3;
                    break;
                case Status.AREA3:
                    targetColor[3] = new(1f, 1f, 1f, 1f);
                    status = Status.AREA4;
                    break;
                case Status.AREA4:
                    targetColor[4] = new(1f, 1f, 1f, 1f);
                    status = Status.AREA5;
                    break;
                case Status.AREA5:
                    targetColor[0] = new(1f, 1f, 1f, 1f);
                    status = Status.AREA1;
                    break;
            }
            targetBackground = backgroundColors[(int)status];
            intervalTimer = intervalTime;
        }

        for (int i = 0; i < 5; i++)
        {
            if (status == Status.AREA4 && i == 3)
            {
                areaTilemaps[i].color = areaTilemaps[i].color + (new Color(0f, 0f, 0f, 1f) - areaTilemaps[i].color) * (chasePower * Time.deltaTime);
                areabgRenderers[i].color = areabgRenderers[i].color + (targetColor[i] - areabgRenderers[i].color) * (chasePower * Time.deltaTime);
            }
            else
            {
                areaTilemaps[i].color = areaTilemaps[i].color + (targetColor[i] - areaTilemaps[i].color) * (chasePower * Time.deltaTime);
                areabgRenderers[i].color = areabgRenderers[i].color + (targetColor[i] - areabgRenderers[i].color) * (chasePower * Time.deltaTime);
            }

        }

        mainCamera.backgroundColor = mainCamera.backgroundColor + (targetBackground - mainCamera.backgroundColor) * (chasePower * Time.deltaTime);

        if (status == Status.AREA4)
        {
            sunRenderer.color = sunRenderer.color + (new Color(1f, 0.7f, 0.4f, 1f) - sunRenderer.color) * (chasePower * Time.deltaTime);
        }
        else
        {
            sunRenderer.color = sunRenderer.color + (new Color(1f, 0.7f, 0.4f, 0f) - sunRenderer.color) * (chasePower * Time.deltaTime);
        }
    }
}
