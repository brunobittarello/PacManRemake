using UnityEngine;

static class StorageManager
{
    const string HIGHSCORE_SLOT = "highscore";
    static bool isLoaded;
    static int highScore;

    internal static int HighScore
    {
        get
        {
            if (isLoaded == false)
                highScore = PlayerPrefs.GetInt(HIGHSCORE_SLOT, 0);

            isLoaded = true;
            return highScore;
        }
        set
        {
            PlayerPrefs.SetInt(HIGHSCORE_SLOT, value);
            PlayerPrefs.Save();
        }
    }
}
