
using System.Collections.Generic;
using UnityEngine;

class Ghost : AnimatedSprite
{
    const float TILE_SNAPPING    = 0.99f;    
    const float SPEED_EYE        = 12;

    internal static bool blinkWhite;

    [SerializeField]
    GhostType type;
    [SerializeField]
    internal GhostState Status { get; private set; }
    internal GhostType Type { get { return type; } }

    [Header("Sprites")]
    SpriteRenderer spriteRenderer;
    public Sprite[] movementsSprites;
    public Sprite[] fearSprites;
    public Sprite[] eyesSprites;


    Direction direction;
    Vector2Int dirVector;
    WalkableTile tile;
    WalkableTile tileTarget;
    float inBetwen;

    float speedNormal;
    float speedFrightened;
    float speedTunnel;

    WalkableTile cornerTile;
    WalkableTile spawnTile;
    bool dismissDirection;
    int frameIndex;

    internal bool IsFrightenedInside { get; private set; }
    internal bool IsAlive { get { return Status == GhostState.Chasing || Status == GhostState.Frightened || Status == GhostState.Scatter; } }
    internal Vector2Int Position { get { return tile.pos; } }

    internal Direction Direction
    {
        get { return direction; }
        private set
        {
            direction = value;
            dirVector = Utils.VectorByDir(direction);
        }
    }

    internal void SetSpeeds(float speedNormal, float speedFrightened, float speedTunnel)
    {
        this.speedNormal = speedNormal;
        this.speedFrightened = speedFrightened;
        this.speedTunnel = speedTunnel;
    }

    #region Unity
    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        SetTicksToChangeFrame(3);
    }

    void Update()
    {
        switch (Status)
        {
            case GhostState.Waiting:    UpdateWaiting(); break;
            case GhostState.InHouse:    UpdateInHouse(); break;
            case GhostState.Exiting:    UpdateExiting(); break;
            case GhostState.Scatter:    UpdateScatter(); break;
            case GhostState.Chasing:    UpdateChasing(); break;
            case GhostState.Frightened: UpdateFrightened(); break;
            case GhostState.Eaten:      UpdateEaten(); break;
        }
    }
    #endregion
    #region Setting
    internal void Hide()
    {
        spriteRenderer.enabled = false;
    }

    internal void Show()
    {
        spriteRenderer.enabled = true;
    }

    internal void SetReady()
    {
        Status = GhostState.Waiting;
        Show();

        switch (type)
        {
            case GhostType.Blinky: SetBlinky(); break;
            case GhostType.Pinky:  SetPinky(); break;
            case GhostType.Inky:   SetInky(); break;
            case GhostType.Clyde:  SetClyde(); break;
        }
        
        inBetwen = 0.5f;
        Direction newDir;
        tileTarget = tile.NextInDirection(direction, out newDir);

        PositionUpdate();
        spriteRenderer.sprite = movementsSprites[2];
    }

    void SetBlinky()
    {
        tile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(16, 19))];
        Direction = Direction.left;

        spawnTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(16, 16))];
        cornerTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(28, 29))];
    }

    void SetPinky()
    {
        tile = spawnTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(16, 16))];
        direction = Direction.down;

        cornerTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(3, 29))];
    }

    void SetInky()
    {
        tile = spawnTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(14, 16))];
        direction = Direction.up;

        cornerTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(28, 1))];
    }

    void SetClyde()
    {
        tile = spawnTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(18, 16))];
        direction = Direction.up;

        cornerTile = StageController.instance.walkableTiles[Utils.TilePosToKey(new Vector2Int(3, 1))];
    }
    #endregion
    #region StatusChanger
    internal void OnGameStart()
    {
        if (type == GhostType.Blinky)
            ChangeToStageMoment();
        else
            StartInHouse();
    }

    void StartInHouse()
    {
        Status = GhostState.InHouse;
    }

    internal void StartExiting()
    {
        Status = GhostState.Exiting;
        IsFrightenedInside = false;
    }

    internal void OnPacManDeath()
    {
        Status = GhostState.Waiting;
        //TODO keep animation
    }
    
    internal void OnPacManPowersUp()
    {
        if (Status == GhostState.Eaten || Status == GhostState.Exiting)
            return;

        if (Status == GhostState.InHouse)
        {
            IsFrightenedInside = true;
            return;
        }
        Status = GhostState.Frightened;
        dismissDirection = true;
    }

    internal void OnPacManWins()
    {
        Status = GhostState.Waiting;
    }

    internal void OnPowerUpIsOver()
    {
        IsFrightenedInside = false;
        if (Status == GhostState.Frightened)
            ChangeToStageMoment();
    }

    internal void OnBeenEaten()
    {
        Status = GhostState.Eaten;
    }

    internal void ChangeToStageMoment(bool force = false)
    {
        if (force == false && (Status == GhostState.Eaten || Status == GhostState.Exiting || Status == GhostState.InHouse))
            return;
        Status = StageController.instance.ghostMoment;
        dismissDirection = true;
    }
    #endregion


    #region Updates
    void UpdateWaiting()
    {
        if (StageController.instance.Status == StageState.PacManDead)
            Animate();
    }

    void UpdateInHouse()
    {
        Move(speedFrightened * Time.deltaTime);
    }

    void UpdateExiting()
    {
        Move(speedFrightened * Time.deltaTime);
    }

    void UpdateScatter()
    {
        Move(CurrentSpeed());
    }

    void UpdateChasing()
    {
        Move(CurrentSpeed());
    }

    void UpdateFrightened()
    {
        Move(speedFrightened * Time.deltaTime);
    }

    void UpdateEaten()
    {
        Move(SPEED_EYE * Time.deltaTime);
    }
    #endregion

    float CurrentSpeed()
    {
        return ((tile.IsTunnel) ? speedTunnel : speedNormal) * Time.deltaTime;
    }

    void Move(float amount)
    {
#if UNITY_EDITOR
        if (StageController.StopGhosts == true)
            return;
#endif
        if (StageController.IsFrozen && (Status != GhostState.Eaten || spriteRenderer.enabled == false))
            return;

        inBetwen += amount;

        if (inBetwen > TILE_SNAPPING)
            OnTileReached();

        PositionUpdate();
        Animate();
    }

    void OnTileReached()
    {
        tile = tileTarget;
        inBetwen -= 1;

        //Wrap
        if (tile.type == TileType.Wrap)
            tile = tile.conns[Utils.ConnIndexByDir(direction)];

        if (dismissDirection == false && tile.connType != ConnType.Multi && Status != GhostState.InHouse)
        {
            Direction newDir;
            tileTarget = tile.NextInDirection(direction, out newDir);
            Direction = newDir;
            return;
        }


        if (Status == GhostState.Chasing)
            ChasePacMan();
        else if (Status == GhostState.Frightened)
            AvoidPacMan();
        else if (Status == GhostState.Eaten)
            FindSpawn();
        else if (Status == GhostState.Exiting)
            FindExit();
        else if (Status == GhostState.InHouse)
            MoveUpAndDown();
        else
            FindCorner();
    }

    #region ChasePacMan
    void ChasePacMan()
    {
        switch (type)
        {
            case GhostType.Blinky: FindPacManAsBlinky(); break;
            case GhostType.Pinky: FindPacManAsPinky(); break;
            case GhostType.Inky: FindPacManAsInky(); break;
            case GhostType.Clyde: FindPackManAsClyde(); break;
        }
    }

    void FindPacManAsBlinky()
    {
        var pacManTile = StageController.instance.walkableTiles[Utils.TilePosToKey(StageController.instance.pacMan.tile)];
        var result = Pathfinder.GetPathAstar(tile, pacManTile, (dismissDirection) ? Vector2Int.zero : dirVector);
        dismissDirection = false;
        TreatPathFindResult(result);
    }

    void FindPacManAsPinky()
    {
        WalkableTile futurePos = null;
        for (int i = 4; i >= 0; i--)
        {
            var key = Utils.TilePosToKey(StageController.instance.pacMan.tile + StageController.instance.pacMan.dirVector * i);
            if (StageController.instance.walkableTiles.ContainsKey(key) && StageController.instance.walkableTiles[key].IsGhostBase == false)
            {
                futurePos = StageController.instance.walkableTiles[key];
                break;
            }
        }

        Debug.DrawLine(transform.parent.TransformPoint((Vector2)tile.pos), transform.parent.TransformPoint((Vector2)futurePos.pos), Color.magenta, 0.25f);


        var result = Pathfinder.GetPathAstar(tile, futurePos, (dismissDirection) ? Vector2Int.zero : dirVector);
        dismissDirection = false;
        TreatPathFindResult(result);
    }

    void FindPacManAsInky()
    {
        WalkableTile futurePos = null;
        for (int i = 2; i >= 0; i--)
        {
            var key = Utils.TilePosToKey(StageController.instance.pacMan.tile + StageController.instance.pacMan.dirVector * i);
            if (StageController.instance.walkableTiles.ContainsKey(key) && StageController.instance.walkableTiles[key].IsGhostBase == false)
            {
                futurePos = StageController.instance.walkableTiles[key];
                break;
            }
        }

        var diff = futurePos.pos - StageController.instance.ghosts[0].tile.pos;
        var targetTile = StageController.instance.FindClosestWalkableTile(futurePos.pos + diff * 2);

        if (targetTile == null)
        {
            Debug.LogWarning("No target tile found");
            FallBackMovement();
            return;
        }

        Debug.DrawLine(transform.parent.TransformPoint((Vector2)tile.pos), transform.parent.TransformPoint((Vector2)targetTile.pos), Color.cyan, 0.25f);
        var result = Pathfinder.GetPathAstar(tile, targetTile, (dismissDirection) ? Vector2Int.zero : dirVector);

        dismissDirection = false;
        TreatPathFindResult(result);
    }

    void FindPackManAsClyde()
    {
        var pacManTile = StageController.instance.walkableTiles[Utils.TilePosToKey(StageController.instance.pacMan.tile)];
        var result = Pathfinder.GetPathAstar(tile, pacManTile, (dismissDirection) ? Vector2Int.zero : dirVector);
        dismissDirection = false;

        if (result != null && result.Count > 8)
            TreatPathFindResult(result);
        else
            FindCorner();
    }

    void TreatPathFindResult(List<WalkableTile> result)
    {
        if (result == null || result.Count == 0)
        {
            Debug.LogWarning("no result found!");
            FallBackMovement();
            return;
        }

        if (result.Count == 1)
        {
            Debug.LogWarning("Same tile!");
            FallBackMovement();
            return;
        }

        Direction newDir;
        if (tile.FindTileInConn(result[1], out newDir))
        {
            tileTarget = result[1];
            Direction = newDir;
        }
        else
        {
            Debug.LogWarning("Conn Invalid!");
            FallBackMovement();
        }
    }
    #endregion

    void MoveUpAndDown()
    {
        tileTarget = tile.conns[Utils.ConnIndexByDir(Direction)];
        if (tileTarget != null && tileTarget.type == TileType.GhostArea)
            return;

        Direction = Utils.InverseDirection(Direction);
        tileTarget = tile.conns[Utils.ConnIndexByDir(Direction)];
    }

    void FindCorner()
    {
        var result = Pathfinder.GetPathAstar(tile, cornerTile, (dismissDirection) ? Vector2Int.zero : dirVector);
        dismissDirection = false;
        TreatPathFindResult(result);
    }

    void AvoidPacMan()
    {
        var pacManTile = StageController.instance.walkableTiles[Utils.TilePosToKey(StageController.instance.pacMan.tile)];
        var result = Pathfinder.GetPathAstar(tile, pacManTile, (dismissDirection) ? Vector2Int.zero : dirVector);
        dismissDirection = false;
        
        if (result == null || result.Count == 0)
        {
            Debug.LogWarning("no result found!");
            FallBackMovement();
            return;
        }

        if (result.Count == 1)
        {
            Debug.LogWarning("Same tile!");
            FallBackMovement();
            return;
        }

        var inverDirInd = Utils.ConnIndexByDir(Utils.InverseDirection(Direction));
        Direction newDir;
        if (tile.FindTileInConn(result[1], out newDir))
        {
            var pacManDirInd = Utils.ConnIndexByDir(newDir);
            for (int i = 0; i < tile.conns.Length; i++)
                if (i != inverDirInd && i != pacManDirInd && tile.conns[i] != null && tile.conns[i].IsGhostBase == false)
                {
                    tileTarget = tile.conns[i];
                    Direction = Utils.DirectionByConnIndex(i);
                    return;
                }
        }
        else
        {
            Debug.LogWarning("Conn Invalid!");
            FallBackMovement();
        }
    }

    void FindSpawn()
    {
        var result = Pathfinder.GetPathAstar(tile, spawnTile, Vector2Int.zero, true);

        if (result != null && result.Count < 2)
        {
            Direction = Direction.up;
            StartInHouse();
        }
        else
            TreatPathFindResult(result);
    }

    void FindExit()
    {
        var result = Pathfinder.GetPathAstar(tile, StageController.instance.ghostAreaExit, Vector2Int.zero, true);

        if (result != null && result.Count < 2)
            ChangeToStageMoment(true);
        else
            TreatPathFindResult(result);
    }

    void FallBackMovement()
    {
        Debug.Log("FallBackMovement");
        Direction newDir;
        tileTarget = tile.NextInDirection(direction, out newDir);
        Direction = newDir;
    }

    void PositionUpdate()
    {
        this.transform.localPosition = Vector2.Lerp(tile.pos, tileTarget.pos, inBetwen);
    }

    #region Animation
    void Animate()
    {
        if (IsFrameAvailable() == false)
            return;

        if (Status == GhostState.Eaten)
        {
            spriteRenderer.sprite = eyesSprites[Utils.ConnIndexByDir(direction)];
            return;
        }

        if (Status == GhostState.Frightened || (Status == GhostState.InHouse && IsFrightenedInside))
        {
            FrightenedFrame();
            return;
        }

        switch (direction)
        {
            case Direction.right: MovementFrame(frameIndex != 0 ? 0 : 1); break;
            case Direction.left:  MovementFrame(frameIndex != 2 ? 2 : 3); break;
            case Direction.up:    MovementFrame(frameIndex != 4 ? 4 : 5); break;
            case Direction.down:  MovementFrame(frameIndex != 6 ? 6 : 7); break;
        }
    }

    void MovementFrame(int frame)
    {
        frameIndex = frame;
        spriteRenderer.sprite = movementsSprites[frameIndex];
    }

    void FrightenedFrame()
    {
        frameIndex = (int)Mathf.Repeat(frameIndex + 1, (blinkWhite) ? 4 : 2);
        spriteRenderer.sprite = fearSprites[frameIndex];
    }
    #endregion
}

enum GhostType
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}

enum GhostState
{
    Waiting,
    InHouse,
    Exiting,
    Scatter,
    Chasing,
    Frightened,
    Eaten,
}