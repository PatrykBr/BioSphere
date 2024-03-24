using System.IO;
using UnityEngine;

public class World
{
    // Properties to store world information
    public string WorldName;
    public string WorldDifficulty;
    public int TimesDied;
    public float TimePlayed;
    public string[] AvailableFeatures;
    public string[] SelectedFeatures;

    // Directory where world data will be stored
    public static string WorldDirectory = Path.Combine(Application.persistentDataPath, "Worlds");
    // C:/Users/Patryk/AppData/LocalLow/DefaultCompany/BioSphere/Worlds
    // C:\Users\patbr\AppData\LocalLow\DefaultCompany\BioSphere\Worlds

    // Constructor for creating a new World instance
    public World(string name, string difficulty)
    {
        WorldName = name;
        WorldDifficulty = difficulty;
        TimesDied = 0;
        TimePlayed = 0f;
        AvailableFeatures = new string[0];
        SelectedFeatures = new string[]
        {
            "Blue_Fins",
            "Green_Body",
            "Red_Eyes"
        }; // Default selected features
    }

    // Read world data from a JSON file
    public static World ReadWorldJSON(string name)
    {
        string jsonString = File.ReadAllText(Path.Combine(WorldDirectory, name + ".json"));
        return JsonUtility.FromJson<World>(jsonString);
    }

    // Write world data to a JSON file
    public static void WriteWorldJSON(World world)
    {
        string jsonString = JsonUtility.ToJson(world);
        string filePath = Path.Combine(WorldDirectory, world.WorldName + ".json");
        File.WriteAllText(filePath, jsonString);
    }
}
