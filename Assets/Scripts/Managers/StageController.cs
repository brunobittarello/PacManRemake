using System;
using System.Collections.Generic;
using UnityEngine;

class StageController : AnimatedSprite
{
    const float START_TIME       = 2.5f;
    const float READY_TIME       = 1.25f;
    const float DEATH_TIME       = 3.2f;
    const float DEATH_DELAY_TIME = 1.15f;
    const float WINNING_TIME     = 2.5f;
    const float GAMEOVER_TIME    = 1.5f;
    const float EATING_TIME      = 1f;

    const int TOTAL_PELLETS = 244;
    const float CATCH_DIST  = 1 * 1;

    const int EXTRA_LIFE_AT = 10000;

    const float FULL_SPEED = 11;


    internal static StageController instance;
    internal static bool StopGhosts;
    internal static bool IsFrozen { get { return instance.timerFreeze > 0; } }

    internal Dictionary<int, WalkableTile> walkableTiles;

    public TextAsset mapText;
    public Transform pelletsContainer;
    public Transform charactersContainer;

    public Stage[] stagesConfig;

    [Header("Sprites")]
    public Sprite[] stageSprites;
    public SpriteRenderer stageRenderer;

    [Header("Fruits")]
    public Sprite[] fruitIcons;
    public int[] fruitPoints;

    [Header("Prefabs")]
    public GameObject prefabPellet;
    public GameObject prefabPowerUp;
    public GameObject prefabPacMan;
    public GameObject[] prefabGhosts;
    public GameObject prefabScore;

    int mapX;
    int mapY;

    internal PacMan pacMan { get; private set; }
    internal Ghost[] ghosts { get; private set; }
    internal GhostState ghostMoment { get; private set; }
    int ghostMomentIndex;
    SpriteRenderer fruit;
    Score scorePopUp;
    internal WalkableTile ghostAreaExit;

    int lives;
    int score;
    int level;
    Stage stage;
    internal StageState Status { get; private set; }
    float timer;
    float timerFreeze;
    float powerUpTimer;
    int pelletsToCollect;
    bool isExtraLifeGiven;

    bool wasInkyReleased;
    bool wasClydeReleased;
    int eatGhostScore;
    internal bool IsGameOver { get; private set; }

    //Debug
    bool isImmortal;

    void Awake()
    {
        instance = this;
        SetTicksToChangeFrame(6);

        LoadMap();

        pelletsContainer.transform.localPosition = charactersContainer.transform.localPosition = new Vector2(mapX + 1, mapY) * -0.5f + Vector2.one * 0.5f;
        SpawnPacMan();
        SpawnGhosts();
        SpawnFruit();
        SpawnScore();
    }

    void Update()
    {
        if (IsFrozen)
        {
            timerFreeze -= Time.deltaTime;
            return;
        }
        switch (Status)
        {
            case StageState.Starting:   UpdateStarting(); break;
            case StageState.Restarting: UpdateRestart(); break;
            case StageState.Playing:    UpdatePlaying(); break;
            case StageState.PacManDead: UpdatePacManDead(); break;
            case StageState.Winning:    UpdateWinning(); break;
            case StageState.GameOver:   UpdateGameOver(); break;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetStage();
            StartRestart();
        }
        if (Input.GetKeyDown(KeyCode.F6))
            pelletsToCollect = 1;
        if (Input.GetKeyDown(KeyCode.F7))
        {
            isImmortal = !isImmortal;
            Debug.LogWarning("Immortal : " + isImmortal);
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            StopGhosts = !StopGhosts;
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            ghostMoment = GhostState.Scatter;
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].ChangeToStageMoment();
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ghostMoment = GhostState.Chasing;
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].ChangeToStageMoment();
        }
        if (Input.GetKeyDown(KeyCode.F11))
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].OnPacManPowersUp();
        if (Input.GetKeyDown(KeyCode.F12))
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].OnBeenEaten();
#endif
    }


    #region MapLoading
    void LoadMap()
    {
        var lines = mapText.text.Split('\n');
        var halfX = lines[0].Length - 2;//Mirrored / Removing '\r'
        int id = 0;

        walkableTiles = new Dictionary<int, WalkableTile>();
        mapX = (halfX * 2) - 1;//35
        mapY = lines.Length;

        for (int x = 0; x < halfX; x++)
            for (int y = 0; y < mapY; y++)
            {
                int.TryParse(lines[mapY - 1 - y][x].ToString(), out id);
                CreateTile(new Vector2Int(x, y), (TileType)id);//0
                CreateTile(new Vector2Int(mapX - x, y), (TileType)id);//Mirror //35 - 0 = 35
            }

        ConnectTiles();
    }

    void CreateTile(Vector2Int tile, TileType id)
    {
        if (id == TileType.Wall) return;

        GameObject prefab = null;
        GameObject go = null;

        if (id == TileType.Pellet || id == TileType.Wrap) prefab = prefabPellet;
        if (id == TileType.Powerup) prefab = prefabPowerUp;

        if (prefab != null)
        {
            go = GameObject.Instantiate(prefab, pelletsContainer);
            go.transform.localPosition = (Vector2)tile;
#if UNITY_EDITOR
            go.name = TileType.Pellet.ToString() + tile.ToString();
#endif
        }

        var wTile = new WalkableTile(tile, id, go);
        if (id == TileType.Tunnel || id == TileType.Wrap)
            wTile.IsTunnel = true;

        if ((tile.x == 14 || tile.x == 17) && (tile.y == 7 || tile.y == 19))
            wTile.IsUpLocked = true;

        walkableTiles.Add(Utils.TilePosToKey(tile), wTile);
    }

    void ConnectTiles()
    {
        int key;
        foreach (var tile in walkableTiles)
        {
            key = Utils.TilePosToKey(tile.Value.pos + Utils.VectorByDir(Direction.up));
            if (walkableTiles.ContainsKey(key)) tile.Value.conns[DirectionInt.UP] = walkableTiles[key];

            key = Utils.TilePosToKey(tile.Value.pos + Utils.VectorByDir(Direction.down));
            if (walkableTiles.ContainsKey(key)) tile.Value.conns[DirectionInt.DONW] = walkableTiles[key];

            key = Utils.TilePosToKey(tile.Value.pos + Utils.VectorByDir(Direction.left));
            if (walkableTiles.ContainsKey(key)) tile.Value.conns[DirectionInt.LEFT] = walkableTiles[key];

            key = Utils.TilePosToKey(tile.Value.pos + Utils.VectorByDir(Direction.right));
            if (walkableTiles.ContainsKey(key)) tile.Value.conns[DirectionInt.RIGHT] = walkableTiles[key];

            if (tile.Value.type == TileType.GhostGate)
                ghostAreaExit = tile.Value.conns[0];
        }

        var wrapTiles = new List<WalkableTile>();
        foreach (var tile in walkableTiles)
        {
            tile.Value.UpdateConnType();
            if (tile.Value.type == TileType.Wrap)
                wrapTiles.Add(tile.Value);
        }
        

        //TODO turn it to a generic
        wrapTiles[0].conns[Utils.ConnIndexByDir(Direction.left)] = wrapTiles[1];
        wrapTiles[1].conns[Utils.ConnIndexByDir(Direction.right)] = wrapTiles[0];
    }

    void SpawnPacMan()
    {
        var go = GameObject.Instantiate(prefabPacMan, charactersContainer);
        pacMan = go.GetComponent<PacMan>();
    }

    void SpawnGhosts()
    {
        ghosts = new Ghost[prefabGhosts.Length];
        for (int i = 0; i < prefabGhosts.Length; i++)
        {
            var go = GameObject.Instantiate(prefabGhosts[i], charactersContainer);
            ghosts[i] = go.GetComponent<Ghost>();

        }
    }

    void SpawnFruit()
    {
        var go = GameObject.Instantiate(prefabPellet, charactersContainer);
        go.transform.localPosition = new Vector2(16, 13);
        go.SetActive(false);
        fruit = go.GetComponent<SpriteRenderer>();
    }

    void SpawnScore()
    {
        var go = GameObject.Instantiate(prefabScore, charactersContainer);
        scorePopUp = go.GetComponent<Score>();
    }
    #endregion

    internal void NewGame()
    {
        this.gameObject.SetActive(true);
        IsGameOver = false;

        lives = 3;
        score = 0;
        level = 0;
        isExtraLifeGiven = false;
        UIController.instance.Set1UPScore(score);
        UIController.instance.SetHightScore(StorageManager.HighScore);
        UIController.instance.SetLives(lives);
        UIController.instance.ClearFruitSlots();
        UIController.instance.labelStagePlayerLabel.SetActive(true);
        UIController.instance.labelReady.SetActive(true);
        UIController.instance.labelGameOver.SetActive(false);

        charactersContainer.gameObject.SetActive(false);

        SoundManager.instance.PlayGameBeginning();

        LevelUp();
    }

    void ResetStage()
    {
        Status = StageState.Starting;
        timer = 0;
        fruit.gameObject.SetActive(false);
        scorePopUp.gameObject.SetActive(false);
        pelletsContainer.gameObject.SetActive(true);
        foreach (var item in walkableTiles)
            if (item.Value.go != null)
                item.Value.go.SetActive(true);

        pelletsToCollect = TOTAL_PELLETS;

        ghostMoment = GhostState.Scatter;
        ghostMomentIndex = 0;

        stageRenderer.sprite = stageSprites[0];

        wasInkyReleased = false;
        wasClydeReleased = false;
    }

    #region Updates
    void UpdateStarting()
    {
        timer += Time.deltaTime;
        if (timer > START_TIME)
            StartRestart();
    }

    void UpdateRestart()
    {
        timer += Time.deltaTime;
        if (timer > READY_TIME)
            StartGame();
    }

    void UpdatePlaying()
    {
        pacMan.Show();
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].Show();

        if (powerUpTimer > 0)
        {
            powerUpTimer -= Time.deltaTime;
            Ghost.blinkWhite = (powerUpTimer < 2);
            if (powerUpTimer < 0)
            {
                pacMan.Speed = FULL_SPEED * stage.PacManSpeed;
                for (int i = 0; i < ghosts.Length; i++)
                    ghosts[i].OnPowerUpIsOver();
            }
        }
        else if (ghostMomentIndex < stage.ghostStates.Length)
        {
            timer += Time.deltaTime;
            if (timer > stage.ghostStates[ghostMomentIndex])
            {
                timer -= stage.ghostStates[ghostMomentIndex];
                ghostMomentIndex++;
                ghostMoment = (ghostMoment == GhostState.Chasing) ? GhostState.Scatter : GhostState.Chasing;
                for (int i = 0; i < ghosts.Length; i++)
                    ghosts[i].ChangeToStageMoment();

                Debug.Log(ghostMoment.ToString() + " " + ((ghostMomentIndex == stage.ghostStates.Length) ? "Indef" : stage.ghostStates[ghostMomentIndex].ToString()));
            }

        }

        ManageGhostSounds();
        ManageGhostRelease();
        VerifyGhostCatchPacMan();
        VerifyPacManCatchFruit();
    }

    bool secondPhaseDone;
    void UpdatePacManDead()
    {
        timer += Time.deltaTime;
        if (timer > DEATH_DELAY_TIME && secondPhaseDone == false)
        {
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].Hide();
            pacMan.OnDeath();
            SoundManager.instance.PlayPacManDeath();
            secondPhaseDone = true;
        }

        if (timer > DEATH_TIME - 0.1f && lives != 0)
            pelletsContainer.gameObject.SetActive(false);

        if (timer > DEATH_TIME)
        {
            secondPhaseDone = false;
            lives--;
            if (lives < 0)
                GameOver();
            else
                StartRestart();
        }
    }

    void UpdateWinning()
    {
        timer += Time.deltaTime;

        if (timer > WINNING_TIME * 0.5 && IsFrameAvailable())
        {
            stageRenderer.sprite = (stageRenderer.sprite == stageSprites[0]) ? stageSprites[1] : stageSprites[0];
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].Hide();
        }
        
        if (timer > WINNING_TIME)
        {
            LevelUp();
            StartRestart();
        }
    }

    void UpdateGameOver()
    {
        timer += Time.deltaTime;
        if (timer > GAMEOVER_TIME)
            IsGameOver = true;
    }
    #endregion

    void AddScore(int addScore)
    {
        UIController.instance.Set1UPScore(score += addScore);
        if (score > StorageManager.HighScore)
        {
            StorageManager.HighScore = score;
            UIController.instance.SetHightScore(score);
        }

        if (isExtraLifeGiven == false && score > EXTRA_LIFE_AT)
            GiveExtraLife();
    }

    void LevelUp()
    {
        stage = stagesConfig[level];
        level++;
        UIController.instance.SetNewFruit(fruitIcons[stage.fruitId]);
        pacMan.Speed = FULL_SPEED * stage.PacManSpeed;
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].SetSpeeds(stage.GhostSpeed * FULL_SPEED, stage.GhostFrightenedSpeed * FULL_SPEED, stage.GhostTunnelSpeed * FULL_SPEED);
        ResetStage();
    }

    void GiveExtraLife()
    {
        lives++;
        isExtraLifeGiven = true;
        SoundManager.instance.PlayExtraLife();
        UIController.instance.SetLives(lives);
    }

    void StartRestart()
    {
        Status = StageState.Restarting;
        timer = 0;

        StopGhosts = false;
        pelletsContainer.gameObject.SetActive(true);
        pacMan.SetReady();
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].SetReady();
        UIController.instance.SetLives(lives);
        UIController.instance.labelReady.SetActive(true);
        UIController.instance.labelGameOver.SetActive(false);
        UIController.instance.labelStagePlayerLabel.SetActive(false);
        charactersContainer.gameObject.SetActive(true);

        fruit.gameObject.SetActive(false);
        Debug.Log("Ready");
    }

    void StartGame()
    {
        Status = StageState.Playing;
        timer = 0;
        UIController.instance.labelReady.SetActive(false);
        UIController.instance.labelGameOver.SetActive(false);
        ghostMoment = GhostState.Scatter;
        ghostMomentIndex = 0;
        pacMan.OnGameStart();
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].OnGameStart();
    }

    void PacManDeath()
    {
        Status = StageState.PacManDead;
        timer = 0;
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].OnPacManDeath();
        pacMan.StopPacMan();

        SoundManager.instance.StopAll();
    }

    void PacManEatAGhost(Ghost ghost)
    {
        ghost.OnBeenEaten();
        
        scorePopUp.SetScore(ghost.Position, eatGhostScore);
        AddScore(eatGhostScore);
        eatGhostScore = eatGhostScore * 2;

        timerFreeze = EATING_TIME;
        ghost.Hide();
        pacMan.Hide();
        SoundManager.instance.PlayPacManEatingGhost();
    }

    void PacManWins()
    {
        Status = StageState.Winning;
        timer = 0;
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].OnPacManWins();
        pacMan.StopPacMan();
        SoundManager.instance.StopAll();
    }

    void GameOver()
    {
        Status = StageState.GameOver;
        timer = 0;
        UIController.instance.labelGameOver.SetActive(true);
    }

    void PacManPowersUp()
    {
        eatGhostScore = 200;
        Ghost.blinkWhite = false;
        pacMan.Speed = FULL_SPEED * stage.PacManPowerSpeed;
        for (int i = 0; i < ghosts.Length; i++)
            ghosts[i].OnPacManPowersUp();
        powerUpTimer = stage.PacManPowerSeconds;
    }

    void EnableFruit()
    {
        fruit.gameObject.SetActive(true);
        fruit.sprite = fruitIcons[stage.fruitId];
    }

    void VerifyPacManCatchFruit()
    {
        if (fruit.gameObject.activeInHierarchy == false || (fruit.transform.localPosition - pacMan.transform.localPosition).sqrMagnitude > CATCH_DIST)
            return;

        fruit.gameObject.SetActive(false);
        AddScore(fruitPoints[stage.fruitId]);
        scorePopUp.SetScore(fruit.transform.localPosition, fruitPoints[stage.fruitId]);
        SoundManager.instance.PlayPacManEatingFruit();
        AddScore(fruitPoints[stage.fruitId]);
    }

    void ManageGhostSounds()
    {
        bool isThereAFrightened = false;
        for (int i = 0; i < ghosts.Length; i++)
            if (ghosts[i].Status == GhostState.Eaten)
            {
                SoundManager.instance.LoopGhostEaten();
                return;
            }
            else if (ghosts[i].Status == GhostState.Frightened)
                isThereAFrightened = true;

        if (isThereAFrightened)
            SoundManager.instance.LoopGhostFrightened();
        else
        {
            float pitch = 1;
            if (pelletsToCollect < 8)
                pitch = 2;
            else if (pelletsToCollect < 16)
                pitch = 1.8f;
            else if (pelletsToCollect < 32)
                pitch = 1.6f;
            else if (pelletsToCollect < 64)
                pitch = 1.4f;
            else if (pelletsToCollect < 128)
                pitch = 1.2f;

            SoundManager.instance.LoopGhostNormal(pitch);
        }
    }

    void ManageGhostRelease()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            switch (ghosts[i].Type)
            {
                case GhostType.Blinky:
                case GhostType.Pinky:
                    if (ghosts[i].Status == GhostState.InHouse && ghosts[i].IsFrightenedInside == false)
                        ghosts[i].StartExiting();
                    break;
                case GhostType.Inky:
                    if (ghosts[i].Status == GhostState.InHouse && ghosts[i].IsFrightenedInside == false && ((wasInkyReleased == false && pelletsToCollect < TOTAL_PELLETS - 30) || (wasInkyReleased && timer > 15)))//TODO 
                    {
                        wasInkyReleased = true;
                        ghosts[i].StartExiting();
                    }
                    break;
                case GhostType.Clyde:
                    if (ghosts[i].Status == GhostState.InHouse && ghosts[i].IsFrightenedInside == false && ((wasClydeReleased == false && pelletsToCollect < TOTAL_PELLETS - 60) || (wasClydeReleased && timer > 30)))
                    {
                        wasClydeReleased = true;
                        ghosts[i].StartExiting();
                    }
                    break;
            }
        }
    }

    void VerifyGhostCatchPacMan()
    {
        if (isImmortal)
            return;

        for (int i = 0; i < ghosts.Length; i++)
            if (ghosts[i].IsAlive && (ghosts[i].transform.localPosition - pacMan.transform.localPosition).sqrMagnitude < CATCH_DIST)
            {
                if (powerUpTimer > 0 && ghosts[i].Status == GhostState.Frightened)
                    PacManEatAGhost(ghosts[i]);
                else
                    PacManDeath();
                return;
            }
    }

    internal bool TryCollectPellet(Vector2Int tilePos)
    {
        var key = Utils.TilePosToKey(tilePos);
        if (walkableTiles.ContainsKey(key) == false)
            return false;

        var type = walkableTiles[key].type;
        if ((type != TileType.Pellet && type != TileType.Powerup) || (walkableTiles[key].go == null || walkableTiles[key].go.activeInHierarchy == false))
            return false;

        walkableTiles[key].go.SetActive(false);
        pelletsToCollect--;
        //Debug.Log(pelletsToCollect);

        SoundManager.instance.PlayPacManEatingPellet();
        AddScore(type == TileType.Powerup ? 50 : 10);

        if (pelletsToCollect == 70 || pelletsToCollect == 170)
            EnableFruit();

        if (pelletsToCollect == 0)
            PacManWins();
        else if (type == TileType.Powerup)
            PacManPowersUp();

        return true;
    }

    internal bool PacManCanMoveThere(Vector2Int tilePos)
    {
        var key = Utils.TilePosToKey(tilePos);
        if (walkableTiles.ContainsKey(key) == false)
            return false;

        var type = walkableTiles[key].type;
        return type == TileType.Pellet || type == TileType.Powerup || type == TileType.Empty || type == TileType.Tunnel;

        //var tile = mapTiles[tilePos.x, tilePos.y];
    }

    internal WalkableTile FindClosestWalkableTile(Vector2Int pos)
    {
        pos = NormalizePosition(pos);
        int key = 0;
        var openList = new List<Vector2Int>();
        var closeList = new List<int>();
        openList.Add(pos);
        while (openList.Count > 0 && openList.Count < 150)
        {
            pos = openList[0];
            key = Utils.TilePosToKey(pos);
            closeList.Add(key);

            if (StageController.instance.walkableTiles.ContainsKey(key))
                return StageController.instance.walkableTiles[key];

            openList.RemoveAt(0);
            key = Utils.TilePosToKey(pos + Vector2Int.up);
            if (openList.Contains(pos + Vector2Int.up) == false && closeList.Contains(key) == false)
                openList.Add(pos + Vector2Int.up);

            key = Utils.TilePosToKey(pos + Vector2Int.down);
            if (openList.Contains(pos + Vector2Int.down) == false && closeList.Contains(key) == false)
                openList.Add(pos + Vector2Int.down);

            key = Utils.TilePosToKey(pos + Vector2Int.left);
            if (openList.Contains(pos + Vector2Int.left) == false && closeList.Contains(key) == false)
                openList.Add(pos + Vector2Int.left);

            key = Utils.TilePosToKey(pos + Vector2Int.right);
            if (openList.Contains(pos + Vector2Int.right) == false && closeList.Contains(key) == false)
                openList.Add(pos + Vector2Int.right);
        }
        return null;
    }

    Vector2Int NormalizePosition(Vector2Int pos)
    {
        var rect = new Rect(0, 0, mapX, mapY);
        var floatPos = Rect.PointToNormalized(rect, pos);
        return new Vector2Int(Mathf.RoundToInt(floatPos.x * mapX), Mathf.RoundToInt(floatPos.y * mapY));
    }
}

enum StageState
{
    Starting,
    Restarting,
    Playing,
    PacManDead,
    Winning,
    GameOver,
}

