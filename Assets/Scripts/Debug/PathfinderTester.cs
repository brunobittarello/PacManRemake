using UnityEngine;

class PathfinderTester : MonoBehaviour
{
    public Vector2Int startPos;
    public Vector2Int endPos;
    public Direction direction;

    [Header("Default")]
    public Transform copyPosition;
    public GameObject pathfilderDebugPrefab;

    [ContextMenu("Test Pathfinder")]
    internal void TestPathfinder()
    {
        var startTile = StageController.instance.walkableTiles[Utils.TilePosToKey(startPos)];
        var endTile = StageController.instance.walkableTiles[Utils.TilePosToKey(endPos)];//26

        if (Application.isPlaying == false || startTile == null || endTile == null)
            return;

        this.transform.localPosition = copyPosition.localPosition;
        Clear();

        var result = Pathfinder.GetPathAstar(startTile, endTile, Utils.VectorByDir(direction), true);
        for (int i = 0; i < result.Count; i++)
        {
            var go = GameObject.Instantiate(pathfilderDebugPrefab, this.transform);
            go.transform.localPosition = (Vector2)result[i].pos;
        }
    }

    [ContextMenu("Clear")]
    internal void Clear()
    {
        var total = this.transform.childCount;
        for (int i = total - 1; i >= 0; i--)
            GameObject.DestroyImmediate(this.transform.GetChild(i).gameObject);
    }
}