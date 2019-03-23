using UnityEngine;

public class CharacterInMenu : AnimatedSprite
{
    public SpriteRenderer sptRenderer;
    public int frameSpeed;
    public Sprite[] animNormal;    

    Sprite[] animationSprite;
    int index;

    void Start()
    {
        SetTicksToChangeFrame(frameSpeed);
        Reset();
    }

    void Update()
    {
        if (IsFrameAvailable() == false)
            return;

        index++;
        if (index >= animationSprite.Length) index = 0;
        sptRenderer.sprite = animationSprite[index];

    }

    private void OnEnable()
    {
        Reset();
    }

    internal void Reset()
    {
        animationSprite = animNormal;
        index = 0;
        sptRenderer.sprite = animationSprite[index];
    }

    internal void ChangeSprites(Sprite[] sprites)
    {
        animationSprite = sprites;
        index = 0;
        sptRenderer.sprite = animationSprite[index];
    }
}
