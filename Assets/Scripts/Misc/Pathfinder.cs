using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static class Pathfinder
{
    const int SEARCH_LIMIT = 500;
    static int maxLoops;
    static bool enableGhostArea;

    public static List<WalkableTile> GetPathAstar(WalkableTile start, WalkableTile end, Vector2Int dir, bool isGhostEaten = false)
    {
        enableGhostArea = isGhostEaten;
        var result = AstarSearch(start, end.pos, dir);
        if (result == null)
            return null;

        return BuildPath(result);
    }

    private static List<WalkableTile> BuildPath(WalkableTile tile)
    {
        var shortPath = new List<WalkableTile>();
        maxLoops = 0;
        while (tile != null && maxLoops < SEARCH_LIMIT)
        {
            maxLoops++;
            shortPath.Insert(0, tile);
            tile = tile.parent;
        }

#if PATHFIND_LOG
        Debug.LogWarning("PATH SIZE " + shortPath.Count);
#endif
        return shortPath;
        //shortPath.Reverse();
    }

    private static WalkableTile AstarSearch(WalkableTile start, Vector2Int endPos, Vector2Int startDir)
    {
        start.SetPathNode(null, endPos);
        start.dir = startDir;
        var openSet = new List<WalkableTile>();
        var closeSet = new List<WalkableTile>();//TODO REMOVE?

        maxLoops = 0;
        openSet.Add(start);
        while (openSet.Count > 0 && maxLoops < SEARCH_LIMIT)
        {
            maxLoops++;
            var tile = openSet[0];
            openSet.RemoveAt(0);
            closeSet.Add(tile);

            if (tile.pos == endPos)//Found!
            {
#if PATHFIND_LOG
                Debug.LogWarning("TILE FOUND " + maxLoops);
#endif
                return tile;
            }

            int i = tile.IsUpLocked ? 1 : 0;//Skip Up node

            for (; i < tile.conns.Length; i++)
            {
                if (tile.conns[i] != null && (enableGhostArea == true || tile.conns[i].IsGhostBase == false) && tile.pos != tile.conns[i].pos + tile.dir && closeSet.Contains(tile.conns[i]) == false && openSet.Contains(tile.conns[i]) == false)
                {
                    tile.conns[i].SetPathNode(tile, endPos);
                    openSet.Add(tile.conns[i]);
                }
            }

            openSet = openSet.OrderBy(n => n.F).ToList();
        }

        if (maxLoops == SEARCH_LIMIT)
            Debug.LogWarning("Limit reached!");
        else
            Debug.LogWarning("Not found!");
        return null;
    }
}
