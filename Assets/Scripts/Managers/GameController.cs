using UnityEngine;
using UnityEngine.Video;

class GameController : MonoBehaviour
{
    const float INTRO_MIN_TIME = 2;

    public VideoPlayer intro;
    public MenuManager menu;
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
        menu.gameObject.SetActive(false);
        stage.gameObject.SetActive(false);
        status = GameState.Intro;

        intro.Play();
    }

    void Update()
    {
        if (status == GameState.Intro)
            UpdateIntro();
        else if (status == GameState.Menu)
            UpdateMenu();
        else if (status == GameState.Game)
            UpdateGame();
    }

    void UpdateIntro()
    {
        timer += Time.deltaTime;
        if (timer > INTRO_MIN_TIME && intro.isPlaying == false)
            StartMenu();
    }

    void UpdateMenu()
    {
        if (Input.anyKey)
            StartGame();
    }

    void UpdateGame()
    {
        if (stage.IsGameOver)
            StartMenu();
    }

    void StartMenu()
    {
        status = GameState.Menu;
        intro.gameObject.SetActive(false);
        stage.gameObject.SetActive(false);
        menu.StartMenu();
    }

    void StartGame()
    {
        status = GameState.Game;
        intro.gameObject.SetActive(false);
        menu.gameObject.SetActive(false);
        stage.NewGame();
    }
}

enum GameState
{
    Intro,
    Menu,
    Demo,
    Game,
}