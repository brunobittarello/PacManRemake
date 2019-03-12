using UnityEngine;
using UnityEngine.Video;

class GameController : MonoBehaviour
{
    const float INTRO_MIN_TIME = 2;

    public VideoPlayer intro;
    public StageController stage;

    GameState status;
    bool isPlaying;
    float timer;

    private void Awake()
    {
        stage.gameObject.SetActive(true);
    }

    private void Start()
    {
        intro.gameObject.SetActive(true);
        stage.gameObject.SetActive(false);
        status = GameState.Intro;

        intro.Play();
    }

    void Update()
    {
        if (status == GameState.Intro)
            UpdateIntro();
        else if (status == GameState.Game)
            UpdateGame();
    }

    void UpdateIntro()
    {
        timer += Time.deltaTime;
        if (timer > INTRO_MIN_TIME && intro.isPlaying == false)
            StartGame();
    }

    void UpdateGame()
    {
        if (stage.IsGameOver)
            BackToMenu();
    }

    void StartGame()
    {
        status = GameState.Game;
        intro.gameObject.SetActive(false);
        stage.NewGame();
    }

    void BackToMenu()
    {        
        status = GameState.Intro;//TODO menu
        intro.gameObject.SetActive(true);
        stage.gameObject.SetActive(false);
        intro.Play();
        timer = 0;
    }
}

enum GameState
{
    Intro,
    Menu,
    Demo,
    Game,
}