#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathfinderTester))]
class PathfinderTesterEditor : Editor
{
    PathfinderTester tester;

    private void OnEnable()
    {
        tester = (PathfinderTester)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (tester == null || EditorApplication.isPlaying == false)
            return;

        if (GUILayout.Button("Custom"))
            tester.TestPathfinder();
        if (GUILayout.Button("Clear"))
            tester.Clear();

        if (GUILayout.Button("Bottom Left To Top Right"))
            BottomLeftToTopRight();
        if (GUILayout.Button("Bottom Right To Top Left"))
            BottomRightToTopLeft();
        if (GUILayout.Button("Top Right To Left Bottom"))
            TopRightToLeftBottom();
        if (GUILayout.Button("Wrap"))
            Wrap();
        if (GUILayout.Button("To Ghost Spawn"))
            ToGhostSpawn();
    }

    void BottomLeftToTopRight()
    {
        tester.startPos = new Vector2Int(3, 1);
        tester.endPos = new Vector2Int(28, 29);
        tester.direction = Direction.down;
        tester.TestPathfinder();
    }

    void BottomRightToTopLeft()
    {
        tester.startPos = new Vector2Int(28, 1);
        tester.endPos = new Vector2Int(3, 29);
        tester.direction = Direction.right;
        tester.TestPathfinder();
    }

    void TopRightToLeftBottom()
    {
        tester.startPos = new Vector2Int(28, 29);
        tester.endPos = new Vector2Int(3, 1);
        tester.direction = Direction.left;
        tester.TestPathfinder();
    }

    void Wrap()
    {
        tester.startPos = new Vector2Int(3, 16);
        tester.endPos = new Vector2Int(28, 16);
        tester.direction = Direction.left;
        tester.TestPathfinder();
    }

    void ToGhostSpawn()
    {
        tester.startPos = new Vector2Int(23, 10);
        tester.endPos = new Vector2Int(16, 16);
        tester.direction = Direction.up;
        tester.TestPathfinder();
    }
}
#endif