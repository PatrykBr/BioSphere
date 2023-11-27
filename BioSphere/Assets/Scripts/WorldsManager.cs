using UnityEngine;
using System.IO;
using System;

public class World
//{
//    public string WorldName;
//    public string WorldDifficulty;

//    public static string WorldDirectory = "C:/Users/Patryk/OneDrive/Documents/Github/BioSphere/BioSphere/Assets/JSON/";

//    public static World ReadWorldJSON(string name)
//    {
//        string jsonString = File.ReadAllText(WorldDirectory + name + ".json");
//        return JsonUtility.FromJson<World>(jsonString);
//    }
//    public static void WriteWorldJSON(World world)
//    {
//        string jsonString = JsonUtility.ToJson(world);
//        string filePath = WorldDirectory + world.WorldName + ".json";
//        File.WriteAllText(filePath, jsonString);
//    }

//    void SampleCast()
//    {
//        // float cake = (float)playerHealth;
//        //  double pie = cake;
//    }
//}

{
    public string WorldName;
    public string WorldDifficulty;
    public int TimesDied;
    public float TimePlayed;

    public static string WorldDirectory = "C:/Users/Patryk/OneDrive/Documents/Github/BioSphere/BioSphere/Assets/JSON/";

    // constructor with default values
    public World(string name, string difficulty)
    {
        WorldName = name;
        WorldDifficulty = difficulty;
        TimesDied = 0;
        TimePlayed = 0f;
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

    void SampleCast()
    {
        // float cake = (float)playerHealth;
        //  double pie = cake;
    }
}