using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Stage : ScriptableObject
{
    public int fruitId;
    [Range(0, 1)]
    public float PacManSpeed;
    [Range(0, 1)]
    public float PacManPowerSpeed;
    public int PacManPowerSeconds;

    [Range(0, 1)]
    public float GhostSpeed;
    [Range(0, 1)]
    public float GhostFrightenedSpeed;
    [Range(0, 1)]
    public float GhostTunnelSpeed;

    public int[] ghostStates;

    [Header("House")]
    public int pelletsToFreeInky;
    public int pelletsToFreeClyde;

    [Header("Elroy")]
    public int pelletsLeftToSpeedUpBlinkyLevel1;
    public float speedUpBlinkyLevel1;
    public int pelletsLeftToSpeedUpBlinkyLevel2;
    public float speedUpBlinkyLevel2;

#if UNITY_EDITOR
    [MenuItem("ScriptAsset/Create Stage Config")]
    static void CreateStage()
    {
        var stage = ScriptableObject.CreateInstance<Stage>();
        AssetDatabase.CreateAsset(stage, "Assets/NewStage.asset");
    }
#endif
}