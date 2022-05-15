using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Library;

public static class Storage
{
    private static readonly string savePath;
    private static readonly SaveData saveData;

    public static Mode LastMode 
    { 
        get => saveData.lastMode;
        set
        {
            saveData.lastMode = value;
            Save();
        }
    }

    public static void CompleteLevel(int level)
    {
        if (saveData.completedLevels.Contains(level))
            return;

        saveData.completedLevels.Add(level);
        Save();
    }

    public static bool IsLevelCompleted(int level)
    {
        if (saveData.completedLevels.Contains(level))
            return true;

        return false;
    }

    static Storage()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        savePath = Path.Combine(Application.persistentDataPath, "Save.json");
#else
        savePath = Path.Combine(Application.dataPath, "Save.json");
#endif

        if (File.Exists(savePath))
            saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
        else
            saveData = new SaveData();
    }

    private static void Save()
    {
        File.WriteAllText(savePath, JsonUtility.ToJson(saveData));
    }

    [Serializable]
    private class SaveData
    {
        public Mode lastMode = Mode.Single;
        public List<int> completedLevels = new();
    }
}
