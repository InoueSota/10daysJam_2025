using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Audio;

public class CameraManager : MonoBehaviour
{
    // 自コンポーネント
    private AudioSource audioSource;

    // 他コンポーネント
    private PlayerManager playerManager;

    // 原点
    private Vector3 originPosition;

    [Header("カメラ移動速度")]
    [SerializeField] private float floatRange;
    [SerializeField] private float addRotateValue;
    private float rotateValue;

    [Header("カメラ覗き速度")]
    [SerializeField] private float peekPower;
    [SerializeField] private float peekRange;

    [Header("カメラシェイク")]
    [SerializeField] private float shakeTime;
    [SerializeField] private float shakePower;
    [SerializeField] private int shakeCount;

    [Header("BGM")]
    [SerializeField] private AudioClip[] bgms;

    void Start()
    {
        // 自コンポーネントの取得
        audioSource = GetComponent<AudioSource>();

        // 他コンポーネントの取得
        playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        
        originPosition = transform.position;

        GameManager gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        if (gameManager.GetAreaName() == "Area1") { audioSource.clip = bgms[0]; }
        else if (gameManager.GetAreaName() == "Area2") { audioSource.clip = bgms[1]; }
        else if (gameManager.GetAreaName() == "Area3") { audioSource.clip = bgms[2]; }
        else if (gameManager.GetAreaName() == "Area4") { audioSource.clip = bgms[3]; }
        else if (gameManager.GetAreaName() == "Area5") { audioSource.clip = bgms[4]; }
        audioSource.Play();
    }

    void Update()
    {
        // カメラ更新処理
        CameraUpdate();
    }

    /// <summary>
    /// カメラ更新処理
    /// </summary>
    void CameraUpdate()
    {
        rotateValue += addRotateValue * Time.deltaTime;

        Vector3 floatPosition = Vector3.zero;

        if (Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f)
        {
            floatPosition = originPosition;
        }
        floatPosition.x += Mathf.Cos(rotateValue) * floatRange;
        floatPosition.y += Mathf.Sin(rotateValue * 2f) * floatRange;

        if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
        {
            Vector3 peekPosition = originPosition;
            peekPosition.x += Input.GetAxisRaw("Horizontal") * peekRange;
            peekPosition.y += Input.GetAxisRaw("Vertical") * peekRange;

            transform.position = transform.position + (peekPosition + floatPosition - transform.position) * (peekPower * Time.deltaTime);
        }
        else
        {
            transform.position = transform.position + (floatPosition - transform.position) * (peekPower * Time.deltaTime);
        }
    }

    // Setter
    public void ShakeCamera() { transform.DOShakePosition(shakeTime, shakePower, shakeCount); }
}
