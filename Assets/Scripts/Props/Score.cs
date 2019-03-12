using UnityEngine;

class Score : MonoBehaviour
{
    const float TIME_TO_LIVE = 1;

    public Sprite[] sprites;
    SpriteRenderer spriteRenderer;
    float timer;

    private void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > TIME_TO_LIVE)
            this.gameObject.SetActive(false);
    }

    internal void SetScore(Vector2 localPos, int score)
    {
        timer = 0;
        this.gameObject.SetActive(true);
        this.transform.localPosition = localPos;

        switch (score)
        {
            case 100: spriteRenderer.sprite = sprites[0]; break;
            case 200: spriteRenderer.sprite = sprites[1]; break;
            case 300: spriteRenderer.sprite = sprites[2]; break;
            case 400: spriteRenderer.sprite = sprites[3]; break;
            case 500: spriteRenderer.sprite = sprites[4]; break;
            case 700: spriteRenderer.sprite = sprites[5]; break;
            case 800: spriteRenderer.sprite = sprites[6]; break;

            case 1000: spriteRenderer.sprite = sprites[7]; break;
            case 1600: spriteRenderer.sprite = sprites[8]; break;
            case 2000: spriteRenderer.sprite = sprites[9]; break;
            case 3000: spriteRenderer.sprite = sprites[10]; break;
            case 5000: spriteRenderer.sprite = sprites[11]; break;
            default: Debug.Log("No sprite for " + score); break;
        }
    }
}