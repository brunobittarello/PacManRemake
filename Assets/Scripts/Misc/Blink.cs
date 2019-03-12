using UnityEngine;

class Blink : AnimatedSprite
{
    const int COOLDOWN = 5;

    SpriteRenderer spriteRenderer;
    int counter;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (IsFrameAvailable())
            counter++;

        if (counter > COOLDOWN)
        {
            counter = 0;
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }
    }
}
