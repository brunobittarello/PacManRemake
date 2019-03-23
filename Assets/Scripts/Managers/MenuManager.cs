using UnityEngine;

public class MenuManager : AnimatedSprite
{
    public Animator animator;
    public TextMesh[] ghostNames;
    public Sprite[] ghostFrightned;
    public Sprite[] score;
    public CharacterInMenu[] characters;
    public SpriteRenderer[] powerUps;

    Sprite powerUpSprite;

    void Awake()
    {
        powerUpSprite = powerUps[0].sprite;
        SetTicksToChangeFrame(6);
    }

    void Update()
    {
        if (IsFrameAvailable() == false)
            return;

        for (int i = 0; i < powerUps.Length; i++)
            powerUps[i].sprite = (powerUps[i].sprite == null) ? powerUpSprite : null;
    }

    internal void StartMenu()
    {
        this.gameObject.SetActive(true);
        animator.Play("MenuAnimation", -1, 0f);
    }

    public void ResetNames()
    {
        ghostNames[0].text = "OIKAKE----";
        ghostNames[1].text = "MACHIBUSE--";
        ghostNames[2].text = "KIMAGURE--";
        ghostNames[3].text = "OTOBOKE---";
    }

    public void BlinkyName()
    {
        ghostNames[0].text = "OIKAKE----\"AKABEI\"";
    }

    public void PinkyName()
    {
        ghostNames[1].text = "MACHIBUSE--\"PINKY\"";
    }

    public void InkyName()
    {
        ghostNames[2].text = "KIMAGURE--\"AOSUKE\"";
    }

    public void ClydeName()
    {
        ghostNames[3].text = "OTOBOKE---\"GUZUTA\"";
    }

    public void StartFrightened()
    {
        for (int i = 1; i < characters.Length; i++)
            characters[i].ChangeSprites(ghostFrightned);
    }

    public void BlinkyDeath()
    {
        characters[1].ChangeSprites(new Sprite[1] { score[0] });
    }

    public void PinkyDeath()
    {
        characters[2].ChangeSprites(new Sprite[1] { score[1] });
    }

    public void InkyDeath()
    {
        characters[3].ChangeSprites(new Sprite[1] { score[2] });
    }

    public void ClydeDeath()
    {
        characters[4].ChangeSprites(new Sprite[1] { score[3] });
    }
}
