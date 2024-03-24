using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureStats
{
    public int health;
    public int speed;
    public int strength;
}

[System.Serializable]
public class CreatureFeature
{
    public string name;
    public CreatureStats stats;
}

[System.Serializable]
public class CreatureFeatureList
{
    public List<CreatureFeature> creatureFeatures;
}

public static class CreatureManager
{
    private static CreatureFeatureList _featureList;

    // Load features from JSON on demand
    private static CreatureFeatureList FeatureList
    {
        get
        {
            if (_featureList == null)
            {
                TextAsset itemsJson = Resources.Load<TextAsset>("creature_features");
                _featureList = JsonUtility.FromJson<CreatureFeatureList>(itemsJson.text);
            }
            return _featureList;
        }
    }

    // Find a feature by name from a collection of items
    public static CreatureFeature FindFeatureInList(string featureName)
    {
        return FeatureList.creatureFeatures.Find(feature => feature.name == featureName);
    }

    // Get features based on the world and type
    public static List<CreatureFeature> GetFeaturesFromWorld(World world, string featureType)
    {
        IEnumerable<string> featureCollection;

        switch (featureType)
        {
            case "AvailableFeatures":
                featureCollection = world.AvailableFeatures;
                break;
            case "SelectedFeatures":
                featureCollection = world.SelectedFeatures;
                break;
            default:
                Debug.LogError("Unsupported feature type: " + featureType);
                return new List<CreatureFeature>();
        }

        List<CreatureFeature> features = new List<CreatureFeature>();
        foreach (string featureName in featureCollection)
        {
            CreatureFeature feature = FindFeatureInList(featureName);
            if (feature != null)
                features.Add(feature);
        }
        return features;
    }

    // Create model for selected features
    public static GameObject CreateSelectedFeatureModel(List<CreatureFeature> features)
    {
        GameObject bodyPrefab = LoadFeaturePrefab(features, "Body");
        if (bodyPrefab == null)
        {
            Debug.LogError("Body feature prefab not found in Resources folder.");
            return null;
        }

        GameObject bodyObject = UnityEngine.Object.Instantiate(bodyPrefab);
        foreach (string keyword in new[] { "Eye", "Fin" })
        {
            GameObject featurePrefab = LoadFeaturePrefab(features, keyword);
            if (featurePrefab != null)
                InstantiateChildFeature(bodyObject.transform, featurePrefab, keyword);
        }
        return bodyObject;
    }

    // Load a feature prefab from resources
    private static GameObject LoadFeaturePrefab(List<CreatureFeature> features, string keyword)
    {
        CreatureFeature feature = features.Find(f => f.name.Contains(keyword));
        return feature != null ? Resources.Load<GameObject>("FeaturePrefabs/" + feature.name) : null;
    }

    // Instantiate child feature on a parent transform
    private static void InstantiateChildFeature(Transform parentTransform, GameObject featurePrefab, string keyword)
    {
        if (featurePrefab == null) return;

        foreach (Transform childTransform in parentTransform)
        {
            if (childTransform.name.Contains(keyword))
            {
                Transform pivotChild = childTransform.Find("Pivot");
                Vector3 position = pivotChild != null ? pivotChild.position : childTransform.position;
                UnityEngine.Object.Instantiate(featurePrefab, position, Quaternion.identity, childTransform);
            }
        }
    }

    // Select a random feature of a given type
    public static CreatureFeature SelectRandomFeature(string type)
    {
        List<CreatureFeature> featuresOfType = FeatureList.creatureFeatures.FindAll(feature => feature.name.Contains(type));
        return featuresOfType.Count > 0 ? featuresOfType[UnityEngine.Random.Range(0, featuresOfType.Count)] : null;
    }

    //DID THIS SO THAT IT FIXES A BUG WITH THE FIN PIVOT MOVEMENT
    //private static void InstantiateChildFeatures(Transform parentTransform, GameObject featurePrefab, string keyword)
    //{
    //    if (featurePrefab == null)
    //        return;

    //    foreach (Transform childTransform in parentTransform)
    //    {
    //        if (childTransform.name.Contains(keyword))
    //        {
    //            Object.Instantiate(featurePrefab, childTransform.position, Quaternion.identity, childTransform);
    //        }
    //    }
    //}
}
