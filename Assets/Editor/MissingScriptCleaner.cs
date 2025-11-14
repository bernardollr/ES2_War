using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

// Small Editor utility to find and remove missing scripts in the active scene.
// Adds menu items under Tools -> Missing Scripts

public static class MissingScriptCleaner
{
    [MenuItem("Tools/Missing Scripts/Find Missing Scripts In Scene")]
    public static void FindMissingInScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int totalFound = 0;
        foreach (var root in roots)
        {
            totalFound += FindMissingInGameObject(root);
        }
        if (totalFound == 0)
            Debug.Log("No missing scripts found in scene: " + scene.name);
        else
            Debug.Log($"Found {totalFound} missing script component(s) in scene: {scene.name}. Use Tools/Missing Scripts/Remove Missing Scripts In Scene to remove them.");
    }

    [MenuItem("Tools/Missing Scripts/Remove Missing Scripts In Scene")]
    public static void RemoveMissingInScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int totalRemoved = 0;
        foreach (var root in roots)
        {
            totalRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
        }
        Debug.Log($"Removed {totalRemoved} missing script component(s) in scene: {scene.name}");
    }

    static int FindMissingInGameObject(GameObject go)
    {
        int found = 0;
        // check this gameobject
        var components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                Debug.LogWarning($"Missing script on GameObject: '{GetFullPath(go)}' ", go);
                found++;
            }
        }

        // recurse children
        foreach (Transform child in go.transform)
        {
            found += FindMissingInGameObject(child.gameObject);
        }
        return found;
    }

    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
