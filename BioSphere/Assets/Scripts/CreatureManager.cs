using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public static List<Feature> GetFeatures(World world)
    {
        List<Feature> features = new List<Feature>();
        foreach (string featureName in world.Features)
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
