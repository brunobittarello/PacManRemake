using UnityEngine;

class SoundManager : MonoBehaviour
{
    internal static SoundManager instance { get; private set; }

    [Header("Game")]
    public AudioClip GameBeginning;
    public AudioClip GameExtraLife;

    [Header("Ghosts")]
    public AudioClip GhostEaten;
    public AudioClip GhostsNormal;
    public AudioClip GhostFrightened;

    [Header("PacMan")]
    public AudioClip PacManDeath;
    public AudioClip PacManEatingFruit;
    public AudioClip PacManEatingGhost;
    public AudioClip PacManEatingPelletWa;
    public AudioClip PacManEatingPelletKa;

    [Header("Audio Players")]
    public AudioSource gameAudio;
    public AudioSource ghostAudio;
    public AudioSource pacmanAudio;

    void Awake()
    {
        instance = this;
    }

    #region Game
    internal void PlayGameBeginning()
    {
        gameAudio.PlayOneShot(GameBeginning);
    }

    internal void PlayExtraLife()
    {
        gameAudio.PlayOneShot(GameExtraLife);
    }

    internal void StopAll()
    {
        gameAudio.Stop();
        ghostAudio.Stop();
        pacmanAudio.Stop();
    }
    #endregion
    #region Ghosts
    internal void LoopGhostNormal(float pitch)
    {
        if (ghostAudio.clip == GhostsNormal)
        {
            ghostAudio.pitch = pitch;
            if (ghostAudio.isPlaying == false)
                ghostAudio.Play();
            return;
        }

        ghostAudio.Stop();
        ghostAudio.clip = GhostsNormal;
        ghostAudio.pitch = pitch;
        ghostAudio.Play();
    }

    internal void LoopGhostEaten()
    {
        if (ghostAudio.clip == GhostEaten)
            return;

        ghostAudio.Stop();
        ghostAudio.clip = GhostEaten;
        ghostAudio.pitch = 1;
        ghostAudio.Play();
    }

    internal void LoopGhostFrightened()
    {
        if (ghostAudio.clip == GhostFrightened)
            return;

        ghostAudio.Stop();
        ghostAudio.clip = GhostFrightened;
        ghostAudio.pitch = 1;
        ghostAudio.Play();
    }
    #endregion
    #region PacMan
    internal void PlayPacManDeath()
    {
        pacmanAudio.PlayOneShot(PacManDeath);
    }

    internal void PlayPacManEatingFruit()
    {
        pacmanAudio.PlayOneShot(PacManEatingFruit);
    }

    internal void PlayPacManEatingGhost()
    {
        pacmanAudio.PlayOneShot(PacManEatingGhost);
    }

    internal void PlayPacManEatingPellet()
    {
        //pacmanAudio.Stop();
        //pacmanAudio.PlayOneShot(PacManEatingPellet);
        
        if (pacmanAudio.isPlaying == false || pacmanAudio.clip == PacManEatingPelletKa )
            pacmanAudio.clip = PacManEatingPelletWa;
        else
            pacmanAudio.clip = PacManEatingPelletKa;

        pacmanAudio.Stop();
        pacmanAudio.Play();
        
    }
    #endregion
}