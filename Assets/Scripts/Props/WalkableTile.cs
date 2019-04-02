using System.Collections.Generic;
using UnityEngine;

class WalkableTile
{
    public Vector2Int pos;
    public TileType type;
    public WalkableTile[] conns;
    public ConnType connType;
    public GameObject go;
    public bool IsTunnel;
    public bool IsUpLocked;

    //Pathfinding

    public Vector2Int dir;
    public WalkableTile parent;

    public int F;
    public int G;
    public int H;

    public WalkableTile(Vector2Int tile, TileType type, GameObject go)
    {
        this.pos = tile;
        this.type = type;
        this.go = go;

        conns = new WalkableTile[4];
    }

    public void UpdateConnType()
    {
        var up    = conns[DirectionInt.UP] != null;
        var down  = conns[DirectionInt.DONW] != null;
        var left  = conns[DirectionInt.LEFT] != null;
        var right = conns[DirectionInt.RIGHT] != null;

        int connCount = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
        if (connCount > 2)
            connType = ConnType.Multi;
        else if ((up && down) || (left && right))
            connType = ConnType.Straight;
        else 
            connType = ConnType.Corner;

#if UNITY_EDITOR
        if (go != null)
            go.name += " conn=" + connCount + " type=" + connType.ToString();
#endif
    }

    internal WalkableTile NextInDirection(Direction dir, out Direction newDir)
    {
        var index = Utils.ConnIndexByDir(dir);
        if (conns[index] != null)
        {
            newDir = dir;
            return conns[index];
        }

        var inverseDir = Utils.InverseDirection(dir);
        if (Direction.up != inverseDir && conns[DirectionInt.UP] != null)
        {
            newDir = Direction.up;
            return conns[DirectionInt.UP];
        }

        if (Direction.down != inverseDir && conns[DirectionInt.DONW] != null)
        {
            newDir = Direction.down;
            return conns[DirectionInt.DONW];
        }

        if (Direction.left != inverseDir && conns[DirectionInt.LEFT] != null)
        {
            newDir = Direction.left;
            return conns[DirectionInt.LEFT];
        }

        if (Direction.right != inverseDir && conns[DirectionInt.RIGHT] != null)
        {
            newDir = Direction.right;
            return conns[DirectionInt.RIGHT];
        }

        newDir = dir;
        return null;
    }

    internal bool FindTileInConn(WalkableTile tile, out Direction dir)
    {
        for (int i = 0; i < conns.Length; i++)
            if (conns[i] == tile)
            {
                dir = Utils.DirectionByConnIndex(i);
                return true;
            }
        dir = Direction.up;
        return false;
    }



    public void SetPathNode(WalkableTile parent, Vector2Int target)
    {
        this.parent = parent;
        if (parent != null)
        {
            dir = pos - parent.pos;
            G = parent.G + 1;
        }
        H = Mathf.RoundToInt(Vector2.Distance(pos, target));
        //var temp = tile.pos - target;
        //H = Mathf.Abs(temp.x) + Mathf.Abs(temp.y);
        F = G + H;
    }

    internal bool IsGhostBase { get { return type == TileType.GhostArea || type == TileType.GhostGate; } }
}

enum TileType : byte
{
    Wall,
    Pellet,
    Powerup,
    Empty,
    GhostArea,
    GhostGate,
    Wrap,
    Tunnel,
}

enum ConnType : byte
{
    Straight,
    Corner,
    Multi,
}
