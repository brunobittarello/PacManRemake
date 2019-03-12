using UnityEngine;

static class Utils
{
    internal static int TilePosToKey(Vector2Int pos)
    {
        return pos.x * 100 + pos.y;
    }

    internal static Vector2Int VectorByDir(Direction dir)
    {
        switch (dir)
        {
            case Direction.right: return Vector2Int.right;
            case Direction.left:  return Vector2Int.left;
            case Direction.up:    return Vector2Int.up;
            case Direction.down:  return Vector2Int.down;
        }
        return Vector2Int.zero;
    }

    internal static Direction DirectionByVec(Vector2Int vec)
    {
        if (vec == Vector2Int.up)    return Direction.up;
        if (vec == Vector2Int.down)  return Direction.down;
        if (vec == Vector2Int.left)  return Direction.left;
        if (vec == Vector2Int.right) return Direction.right;

        Debug.Log("Vector not normalized " + vec);
        return Direction.right;
    }

    internal static Direction InverseDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.right: return Direction.left;
            case Direction.left:  return Direction.right;
            case Direction.up:    return Direction.down;
            case Direction.down:  return Direction.up;
        }
        return Direction.down;
    }

    internal static int ConnIndexByDir(Direction dir)
    {
        switch (dir)
        {
            case Direction.up:    return DirectionInt.UP;
            case Direction.down:  return DirectionInt.DONW;
            case Direction.left:  return DirectionInt.LEFT;
            case Direction.right: return DirectionInt.RIGHT;
        }
        return 0;
    }

    internal static Direction DirectionByConnIndex(int index)
    {
        if (index == DirectionInt.UP)    return Direction.up;
        if (index == DirectionInt.DONW)  return Direction.down;
        if (index == DirectionInt.LEFT)  return Direction.left;
        if (index == DirectionInt.RIGHT) return Direction.right;

        return Direction.right;
    }
}

public enum Direction
{
    right,
    left,
    up,
    down
}

public class DirectionInt
{
    public const int UP = 0;
    public const int DONW = 1;
    public const int LEFT = 2;
    public const int RIGHT = 3;
}