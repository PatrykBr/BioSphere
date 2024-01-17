using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class World
{
    public string WorldName;
    public string WorldDifficulty;
    public int TimesDied;
    public float TimePlayed;
    public string[] Features;
    public string[] SelectedFeatures;

    public static string WorldDirectory = Path.Combine(Application.persistentDataPath, "Worlds");
    // C:/Users/Patryk/AppData/LocalLow/DefaultCompany/BioSphere\Worlds
    public World(string name, string difficulty)
    {
        WorldName = name;
        WorldDifficulty = difficulty;
        TimesDied = 0;
        TimePlayed = 0f;
        Features = new string[0];
        SelectedFeatures = new string[] { "Small_Fins", "Small_Body", "Small_Eyes" };
    }

    public static World ReadWorldJSON(string name)
    {
        string jsonString = File.ReadAllText(Path.Combine(WorldDirectory, name + ".json"));
        return JsonUtility.FromJson<World>(jsonString);
    }

    public static void WriteWorldJSON(World world)
    {
        string jsonString = JsonUtility.ToJson(world);
        string filePath = Path.Combine(WorldDirectory, world.WorldName + ".json");
        File.WriteAllText(filePath, jsonString);
    }
}
