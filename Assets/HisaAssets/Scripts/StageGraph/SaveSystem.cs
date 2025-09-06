using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string RootDir => Path.Combine(Application.persistentDataPath, "Saves");
    private static string PathOf(int slot) =>
        Path.Combine(RootDir, $"slot_{slot}.json");

    public static bool Exists(int slot) => File.Exists(PathOf(slot));

    public static void Save(SaveData data, int slot)
    {
        Directory.CreateDirectory(RootDir);
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(PathOf(slot), json);
        Debug.Log($"Saved to {PathOf(slot)}");
    }

    public static SaveData Load(int slot)
    {
        string p = PathOf(slot);
        if (!File.Exists(p))
        {
            Debug.LogWarning($"No save file at {p}");
            return null;
        }
        string json = File.ReadAllText(p);
        var data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"Loaded from {p}");
        return data;
    }

    public static void Delete(int slot)
    {
        string p = PathOf(slot);
        if (File.Exists(p)) File.Delete(p);
    }
}
