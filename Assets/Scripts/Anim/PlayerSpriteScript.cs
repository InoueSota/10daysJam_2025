using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerSpriteScript : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator  = null;

    [SerializeField] int direction = 0;

    bool isLeft = false;

    [SerializeField] private int sliceWidth = 32;   // êÿÇËï™ÇØÇÈïù
    [SerializeField] private int sliceHeight = 32;  // êÿÇËï™ÇØÇÈçÇÇ≥

    private Dictionary<string, Sprite[]> animDict;

    [System.Serializable]
    public struct AnimationSprite
    {
        public string name;
        public Texture2D texture;
        public Sprite[] sprites;
    }

    [SerializeField] private AnimationSprite[] animations;

    public AnimationSprite? GetAnimationByName(string animName)
    {
        foreach (var anim in animations)
        {
            if (anim.name == animName)
                return anim;
        }
        return null; // å©Ç¬Ç©ÇÁÇ»Ç©Ç¡ÇΩèÍçá
    }


    void Awake()
    {
        SliceAllAnimations();
    }

    void SliceAllAnimations()
    {
        for (int i = 0; i < animations.Length; i++)
        {
            Texture2D texture = animations[i].texture;
            if (texture == null) continue;

            List<Sprite> spriteList = new List<Sprite>();

            int cols = texture.width / sliceWidth;
            int rows = texture.height / sliceHeight;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rect rect = new Rect(x * sliceWidth, texture.height - ((y + 1) * sliceHeight), sliceWidth, sliceHeight);
                    Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.25f), 16f);
                    spriteList.Add(sprite);
                }
            }

            animations[i].sprites = spriteList.ToArray();
            //Debug.Log($"{animations[i].name} Çï™äÑäÆóπÅI {animations[i].sprites.Length} ñáÇ…êÿÇËï™ÇØÇ‹ÇµÇΩÅB");
        }
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {

        ChangeSprite();

        
    }

    private void ChangeSprite()
    {


        string baseName = "";

        if (spriteRenderer.sprite != null)
        {
            baseName = spriteRenderer.sprite.name;

            Sprite animSprite = spriteRenderer.sprite;


            // "_" Ç≈ï™äÑ
            string[] parts = baseName.Split('_');

            string animName = parts[0]; // Stand, Walk Ç»Ç«
            int num = (parts.Length > 1) ? int.Parse(parts[1]) : 0; // ññîˆî‘çÜ


            if (animName == "Dash"  && (direction == 1 || direction == 3))
            {
                if (direction == 1) animName += "Up"; 
                else if (direction == 3) animName += "Down";

                AnimationSprite? animBase = GetAnimationByName(animName);

                if (animBase != null)
                {
                    spriteRenderer.sprite = animBase.Value.sprites[num];
                }

            }else if (animName == "Cut" && (direction == 2 || direction == 0))
            {
                animName += "Up";

                AnimationSprite? animBase = GetAnimationByName(animName);

                if (animBase != null)
                {
                    spriteRenderer.sprite = animBase.Value.sprites[num];
                }

            }


            spriteRenderer.flipX = isLeft;



            //Debug.Log($"animName={animName}, num={num}");

        }

       // Sprite[] currentAnim = animDict[animName];
    }

    public void ChangeColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetDirection(int direction_)
    {
        direction = direction_;

        if (direction == 0) isLeft = false;
        else if(direction == 2) isLeft = true;
    }

    public void SetLeft(bool left)
    {
        isLeft = left;
    }
}
