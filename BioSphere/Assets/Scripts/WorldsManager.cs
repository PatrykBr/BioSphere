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

    public static string WorldDirectory = Path.Combine(Application.dataPath, "Resources", "Worlds");


    //public static string WorldDirectory = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Worlds\\";

    public World(string name, string difficulty)
    {
        WorldName = name;
        WorldDifficulty = difficulty;
        TimesDied = 0;
        TimePlayed = 0f;
        Features = new string[0];
    }

    public static World ReadWorldJSON(string name)
    {
        string jsonString = File.ReadAllText(WorldDirectory + name + ".json");
        return JsonUtility.FromJson<World>(jsonString);
    }

    public static void WriteWorldJSON(World world)
    {
        string jsonString = JsonUtility.ToJson(world);
        string filePath = WorldDirectory + world.WorldName + ".json";
        File.WriteAllText(filePath, jsonString);
    }
}
