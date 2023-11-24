using UnityEngine;
using System.IO;
using System;

public class Player
{
    public string playerName;
    public int playerLevel;
    public bool playerIsAlive;
    public double playerHealth;
    public double[] playerPosition;

    public static string PlayerDirectory = "C:/Users/Patryk/OneDrive/BioSphere/Assets/JSON/";
    public static Player ReadPlayerJSON(string name)
    {
        string jsonString = File.ReadAllText(PlayerDirectory + name + ".json");
        return JsonUtility.FromJson<Player>(jsonString);
    }
    public static void WritePlayerJSON(Player player)
    {
        string jsonString = JsonUtility.ToJson(player);
        string filePath = PlayerDirectory + player.playerName + ".json";
        File.WriteAllText(filePath, jsonString);
    }

    void SampleCast()
    {
        float cake = (float)playerHealth;
        double pie = cake;
    }
}