using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public int health;
    public int speed;
    public int strength;
}

[System.Serializable]
public class Feature
{
    public string name;
    public Stat stat;
    public string sprite;
}

[System.Serializable]
public class FeaturesList
{
    public List<Feature> creatureFeatures;
}

public static class FeatureFinder
{
    public static Feature FindFeatureInItems(string featureName)
    {
        TextAsset itemsJson = Resources.Load<TextAsset>("items");
        FeaturesList featuresList = JsonUtility.FromJson<FeaturesList>(itemsJson.text);
        foreach (Feature feature in featuresList.creatureFeatures)
        {
            if (feature.name == featureName)
            {
                return feature;
            }
        }

        return null;
    }

    public static List<Feature> GetFeatures(World world, string type)
    {
        List<Feature> features = new();

        IEnumerable<string> featureCollection;

        // Determine which collection to iterate through based on the 'type' parameter
        if (type == "Features")
        {
            featureCollection = world.Features;
        }
        else if (type == "SelectedFeatures")
        {
            featureCollection = world.SelectedFeatures;
        }
        else
        {
            // Handle unsupported type
            Debug.LogError("Unsupported feature type: " + type);
            return features;
        }

        // Iterate through the selected collection
        foreach (string featureName in featureCollection)
        {
            Feature feature = FindFeatureInItems(featureName);
            if (feature != null)
            {
                features.Add(feature);
            }
        }

        return features;
    }

}
