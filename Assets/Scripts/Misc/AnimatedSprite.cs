using UnityEngine;

class AnimatedSprite : MonoBehaviour
{
    float animationRate;
    float animationTimer;

    public AnimatedSprite()
    {
        animationRate = 0.033f;
    }

    protected void SetTicksToChangeFrame(float rate)
    {
        animationRate = 0.033f * rate;
    }

    protected bool IsFrameAvailable()
    {
        animationTimer += Time.deltaTime;
        if (animationTimer < animationRate)
            return false;

        animationTimer -= animationRate;
        return true;
    }
}
