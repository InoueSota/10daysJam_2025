using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem.XR;

public class AutoKeyChangerScript : MonoBehaviour
{

    [SerializeField] SpriteRenderer buttonSpriteRenderer;
    [SerializeField] Image buttonImage;
    //SpriteRenderer spriteRenderer;

   [SerializeField]  bool isController = false;
    bool isImage = false;
    bool isSpriteSet = true;

    [SerializeField] bool isObjectEnabledPushUI = false;

    enum Button
    {
       Z,
       Space,
       J,
       K,
       R,
       Escape,
    }

    [SerializeField] Button button;

    [SerializeField] private int sliceWidth = 32;   // êÿÇËï™ÇØÇÈïù
    [SerializeField] private int sliceHeight = 32;  // êÿÇËï™ÇØÇÈçÇÇ≥;

    [SerializeField] private int pixelPerUnit = 16;  // êÿÇËï™ÇØÇÈçÇÇ≥;

    [SerializeField] private float pivotX = 0.5f;
    [SerializeField] private float pivotY = 0.5f;

    [SerializeField] private float sizeMultiPer = 1.0f;

    [System.Serializable]
    public struct AnimationSprite
    {
        public string name;
        public Texture2D texture;
        public Sprite[] sprites;
    }

    [SerializeField] private AnimationSprite[] keys;

    void Awake()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        //buttonSpriteRenderer.enabled = false;

        if (buttonImage != null) isImage = true;
        else if (buttonSpriteRenderer != null) isImage = false;
        else isSpriteSet = false;

        SliceAllAnimations();

        StartCoroutine(CheckForControllers());

        if (isObjectEnabledPushUI == true) ChangeSprite();
    }

    void SliceAllAnimations()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            Texture2D texture = keys[i].texture;
            if (texture == null) continue;

            List<Sprite> spriteList = new List<Sprite>();

            int cols = texture.width / sliceWidth;
            int rows = texture.height / sliceHeight;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rect rect = new Rect(x * sliceWidth, texture.height - ((y + 1) * sliceHeight), sliceWidth, sliceHeight);
                    Sprite sprite = Sprite.Create(texture, rect, new Vector2(pivotX, pivotY), pixelPerUnit);
                    spriteList.Add(sprite);
                }
            }

            keys[i].sprites = spriteList.ToArray();
            //Debug.Log($"{animations[i].name} Çï™äÑäÆóπÅI {animations[i].sprites.Length} ñáÇ…êÿÇËï™ÇØÇ‹ÇµÇΩÅB");
        }
    }
    IEnumerator CheckForControllers()
    {
        while (true)
        {
            var controllers = Input.GetJoystickNames();

            if (!isController && controllers.Length > 0)
            {
                isController = true;
                Debug.Log("Connected");

            }
            else if (isController && controllers.Length == 0)
            {
                isController = false;
                Debug.Log("Disconnected");
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void LateUpdate()
    {
        if (isObjectEnabledPushUI == false)
        {
            ChangeSprite();
        }
    }

    private void ChangeSprite()
{

    if (isController == false && isSpriteSet == true)
    {
        string baseName = "";

        if (isImage == true) baseName = buttonImage.sprite.name;
        else baseName = buttonSpriteRenderer.sprite.name;

        // "_" Ç≈ï™äÑ
        string[] parts = baseName.Split('_');

        int num = (parts.Length > 1) ? int.Parse(parts[1]) : 0; // ññîˆî‘çÜ

        Debug.Log(num);

            if (isImage == true)
            {
                buttonImage.sprite = keys[(int)button].sprites[num];
                buttonImage.gameObject.transform.localScale *= sizeMultiPer;
            }
            else
            {
                buttonSpriteRenderer.sprite = keys[(int)button].sprites[num];
                buttonSpriteRenderer.gameObject.transform.localScale *= sizeMultiPer;

            }

        }
    }

}
