using UnityEngine;

class PacMan : AnimatedSprite
{
    const float CORNERING       = 0.65f;
    const float EAT_RANGE       = 0.5f;
    const float TILE_SNAPPING   = 0.99f;
    const int EAT_FRAMES        = 2;
    const float EAT_FREEZE_TIME = 0.0166f;

    [Header("Animations")]
    public Sprite[] animationMoving;
    public Sprite[] animationDying;

    [Header("Debug")]
    [SerializeField]
    PacManState status;
    public Direction direction;
    public Vector2Int dirVector;
    public Vector2Int tile;
    public Vector2Int tileTarget;
    public float inBetwen;

    public Direction? queuedDirection;


    SpriteRenderer sptRenderer;
    int frameIndex;
    bool tileVerified;
    int eatingFrames;
    float eatingFreezeTimer;

    internal float Speed { get; set; }
    bool IsStoped { get { return tile == tileTarget; } }

    #region Unity
    void Awake()
    {
        //QualitySettings.vSyncCount = 0; Application.targetFrameRate = 12;//TODO remove!!!
        sptRenderer = GetComponent<SpriteRenderer>();
        status = PacManState.Waiting;
    }

    void Update()
    {
        switch (status)
        {
            case PacManState.Alive: UpdateAlive(); break;
            case PacManState.Dead:  UpdateDying(); break;
        }
    }
    #endregion
    #region StatusChanger
    internal void SetReady()
    {
        status = PacManState.Waiting;
        sptRenderer.enabled = true;
        direction = Direction.left;
        dirVector = Utils.VectorByDir(direction);
        tile = new Vector2Int(16, 7);
        inBetwen = 0.5f;
        tileTarget = tile + dirVector;
        frameIndex = 0;

        PositionUpdate();
        Rotate();
        sptRenderer.sprite = animationMoving[frameIndex];
    }

    internal void OnGameStart()
    {
        status = PacManState.Alive;
        SetTicksToChangeFrame(1);
    }

    internal void StopPacMan()
    {
        status = PacManState.Waiting;
    }

    internal void OnDeath()
    {
        status = PacManState.Dead;
        direction = Direction.right;
        Rotate();
        SetTicksToChangeFrame(2);
        frameIndex = 0;
        sptRenderer.sprite = animationDying[0];
    }
    #endregion
    #region Updates
    void UpdateAlive()
    {
        if (StageController.IsFrozen)
            return;

        ManageInputs();
        Move();
        AnimationMoving();
    }

    void UpdateDying()
    {
        AnimationDying();
    }
    #endregion

    void ManageInputs()
    {
        var x = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        var y = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));

        if ((x == 0 && y == 0) || (x == 0 && y == 0))
            return;

        if (x == 1)       ChangeDirection(Direction.right);
        else if (x == -1) ChangeDirection(Direction.left);
        else if (y == 1)  ChangeDirection(Direction.up);
        else if (y == -1) ChangeDirection(Direction.down);
    }

    #region Movement
    void ChangeDirection(Direction dir)
    {
        queuedDirection = null;
        if (direction == dir)
            return;

        var newDirVector = Utils.VectorByDir(dir);

        if (dirVector + newDirVector != Vector2.zero)
        {
            queuedDirection = dir;
            return;
        }

        //Is inverse
        direction = dir;

        if (IsStoped)
        {
            tileTarget = tile + newDirVector;
        }
        else
        {
            var dirAux = tile;
            tile = tileTarget;
            tileTarget = dirAux;
        }

        dirVector = newDirVector;
        inBetwen = 1 - inBetwen;

        tileVerified = false;
        Rotate();
    }

    void Move()
    {
        float speedDelta = Time.deltaTime * Speed;

        if (eatingFreezeTimer > 0)
        {
            eatingFreezeTimer -= speedDelta;
            return;
        }

        if (IsStoped)
            inBetwen = 1;
        else
            inBetwen += speedDelta;

        if (queuedDirection != null && inBetwen > CORNERING)
            TryChangeWay(tileTarget);

        if (tileVerified == false && inBetwen > EAT_RANGE)
            TryToCollectCoin();
        if (IsStoped == false && inBetwen > TILE_SNAPPING)
            OnTileReached();

        PositionUpdate();
    }

    void TryChangeWay(Vector2Int closeTile)
    {
        Vector2Int wantedVecDir = Utils.VectorByDir(queuedDirection.Value);
        Vector2Int wantedTile = closeTile + wantedVecDir;
        if (StageController.instance.PacManCanMoveThere(wantedTile) == false)
            return;

        tile = closeTile;
        tileTarget = wantedTile;
        direction = queuedDirection.Value;
        dirVector = wantedVecDir;
        inBetwen -= 1;

        tileVerified = false;
        queuedDirection = null;
        Rotate();
    }

    void TryToCollectCoin()
    {
        tileVerified = true;
        if (StageController.instance.TryCollectPellet(tileTarget) == false)
            return;

        eatingFrames = EAT_FRAMES;
        eatingFreezeTimer = EAT_FREEZE_TIME;
        frameIndex = 0;
    }

    void OnTileReached()
    {
        tile = tileTarget;
        inBetwen -= 1;

        //Wrap
        var tileTile = StageController.instance.walkableTiles[Utils.TilePosToKey(tile)];
        if (tileTile.type == TileType.Wrap)
        {
            tileTile = tileTile.conns[Utils.ConnIndexByDir(direction)];
            tile = tileTile.pos;
        }

        var target = tileTile.conns[Utils.ConnIndexByDir(direction)];
        if (target == null || target.IsGhostBase)
        {
            tileVerified = false;
            tileTarget = tile;
            return;
        }

        tileTarget = target.pos;
        tileVerified = false;
    }

    void PositionUpdate()
    {
        this.transform.localPosition = Vector2.Lerp(tile, tileTarget, inBetwen);
    }

    void Rotate()
    {
        if (direction == Direction.right) this.transform.rotation = Quaternion.identity;
        else if (direction == Direction.up) this.transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (direction == Direction.left) this.transform.rotation = Quaternion.Euler(0, 0, 180);
        else if (direction == Direction.down) this.transform.rotation = Quaternion.Euler(0, 0, 270);
    }
    #endregion

    #region Animation
    internal void Hide()
    {
        sptRenderer.enabled = false;
    }

    internal void Show()
    {
        sptRenderer.enabled = true;
    }

    void AnimationMoving()
    {
        if (IsFrameAvailable() == false || (IsStoped && frameIndex == 0)) return;

        if (eatingFrames > 0)
        {
            sptRenderer.sprite = animationMoving[0];
            eatingFrames--;
            return;
        }

        sptRenderer.sprite = animationMoving[frameIndex];
        frameIndex = (int)Mathf.Repeat(frameIndex + 1, animationMoving.Length);
    }

    void AnimationDying()
    {
        if (frameIndex == -1 || IsFrameAvailable() == false)
            return;

        int frame = Mathf.FloorToInt(frameIndex * 0.5f);
        sptRenderer.sprite = animationDying[frame];
        frameIndex++;
        if (frameIndex == animationDying.Length * 2)
            frameIndex = -1;
    }
    #endregion
}

public enum PacManState
{
    Waiting,
    Alive,
    Dead
}