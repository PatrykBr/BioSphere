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

    public static string WorldDirectory = Application.persistentDataPath + "/worlds/";
    // C:/Users/patbr/AppData/LocalLow/DefaultCompany/BioSphere/worlds/
    // constructor with default values
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
        // Debug.Log(WorldDirectory);

        string jsonString = JsonUtility.ToJson(world);
        string filePath = WorldDirectory + world.WorldName + ".json";
        File.WriteAllText(filePath, jsonString);
    }

    void SampleCast()
    {
        // float cake = (float)playerHealth;
        //  double pie = cake;
    }
}