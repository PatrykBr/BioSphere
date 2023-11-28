using System.Collections.Generic;
using UnityEngine;

public class Creature
{
    public string[] Features;

    public static Creature CalcStats(string name)
    {
        // Use World.ReadWorldJSON(name) to get the World object
        World worldData = World.ReadWorldJSON(name);

        // Create a new Creature object and populate its properties
        Creature creature = new Creature
        {
            Features = worldData.Features
        };

        // Log the features (replace this with your actual logic)
        Debug.Log("Creature Features: " + string.Join(", ", creature.Features));

        // Load features from items.json
        creature.LoadFeaturesFromJSON();

        return creature;
    }

    private void LoadFeaturesFromJSON()
    {
        // Load the JSON file (you may need to adjust the path based on your project structure)
        TextAsset jsonFile = Resources.Load<TextAsset>("items");
        if (jsonFile == null)
        {
            Debug.LogError("Failed to load items.json");
            return;
        }

        // Parse JSON data
        CreatureFeatures creatureFeatures = JsonUtility.FromJson<CreatureFeatures>(jsonFile.text);

        // Access and process features based on the creature's Features array
        foreach (string featureType in this.Features)
        {
            if (creatureFeatures.creatureFeatures.ContainsKey(featureType))
            {
                Feature[] features = creatureFeatures.creatureFeatures[featureType];
                foreach (Feature feature in features)
                {
                    Debug.Log($"Feature: {feature.name}, Unlocked: {feature.unlocked}");

                    // Access and process stat values
                    foreach (KeyValuePair<string, int> stat in feature.stat)
                    {
                        Debug.Log($"Stat: {stat.Key}, Value: {stat.Value}");
                    }

                    Debug.Log($"Sprite: {feature.sprite}");
                }
            }
        }
    }
}

[System.Serializable]
public class CreatureFeatures
{
    public Dictionary<string, Feature[]> creatureFeatures;
}

[System.Serializable]
public class Feature
{
    public string name;
    public bool unlocked;
    public Dictionary<string, int> stat;
    public string sprite;
}
