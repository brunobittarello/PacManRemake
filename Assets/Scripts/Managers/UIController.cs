using UnityEngine;

class UIController : AnimatedSprite
{
    internal static UIController instance;

    public GameObject labelPlayer1;
    public GameObject labelStagePlayerLabel;
    public GameObject labelReady;
    public GameObject labelGameOver;

    public TextMesh textHighScore;
    public TextMesh textPlayer1Score;
    public Transform containerlifeSlot;
    public Transform containerFruitSlot;

    GameObject[] livesSlots;
    SpriteRenderer[] fruitSlots;

    private void Awake()
    {
        instance = this;
        livesSlots = new GameObject[containerlifeSlot.childCount];
        for (int i = 0; i < containerlifeSlot.childCount; i++)
            livesSlots[i] = containerlifeSlot.GetChild(i).gameObject;
        fruitSlots = containerFruitSlot.GetComponentsInChildren<SpriteRenderer>();

        labelGameOver.SetActive(false);

    }

    private void Update()
    {
        if (IsFrameAvailable() == false)
            return;

        SetTicksToChangeFrame(6);
        labelPlayer1.SetActive(!labelPlayer1.activeInHierarchy);
    }

    internal void Set1UPScore(int score)
    {
        if (score == 0)
            textPlayer1Score.text = "00";
        else
            textPlayer1Score.text = score.ToString();
    }

    internal void SetHightScore(int score)
    {
        if (score == 0)
            textHighScore.text = "";
        else
            textHighScore.text = score.ToString();
    }

    internal void SetLives(int lives)
    {
        for (int i = 0; i < livesSlots.Length; i++)
            livesSlots[i].SetActive(i < lives);
    }

    internal void SetNewFruit(Sprite fruit)
    {
        for (int i = 0; i < fruitSlots.Length; i++)
            if (fruitSlots[i].sprite == null)
            {
                fruitSlots[i].gameObject.SetActive(true);
                fruitSlots[i].sprite = fruit;
                return;
            }

        Sprite aux;
        for (int i = fruitSlots.Length - 1; i >= 0; i--)
        {
            aux = fruitSlots[i].sprite;
            fruitSlots[i].sprite = fruit;
            fruit = aux;
        }
    }

    internal void ClearFruitSlots()
    {
        for (int i = 0; i < fruitSlots.Length; i++)
        {
            fruitSlots[i].sprite = null;
            fruitSlots[i].gameObject.SetActive(false);
        }
    }
}