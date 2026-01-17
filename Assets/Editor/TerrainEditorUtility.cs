using UnityEngine;
using UnityEditor;

public static class TerrainEditorUtility
{
    // ---------------------------
    // Generate Random Heights
    // ---------------------------
    [MenuItem("Terrain/Generate Random Heights", true)]
    public static bool ValidateGenerateRandomHeights()
    {
        return Selection.activeGameObject?.GetComponent<CustomTerrain>() != null;
    }

    [MenuItem("Terrain/Generate Random Heights")]
    public static void GenerateRandomHeights()
    {
        CustomTerrain terrain = Selection.activeGameObject.GetComponent<CustomTerrain>();
        terrain.RandomTerrain();
        Debug.Log("Random heights generated for " + terrain.name);
    }

    // ---------------------------
    // Reset Terrain
    // ---------------------------
    [MenuItem("Terrain/Reset Terrain", true)]
    public static bool ValidateResetTerrain()
    {
        return Selection.activeGameObject?.GetComponent<CustomTerrain>() != null;
    }

    [MenuItem("Terrain/Reset Terrain")]
    public static void ResetTerrain()
    {
        CustomTerrain terrain = Selection.activeGameObject.GetComponent<CustomTerrain>();
        terrain.ResetTerrain();
        Debug.Log("Terrain reset for " + terrain.name);
    }

    // ---------------------------
    // Load Texture Heights
    // ---------------------------
    [MenuItem("Terrain/Load Texture Heights", true)]
    public static bool ValidateLoadTextureHeights()
    {
        var terrain = Selection.activeGameObject?.GetComponent<CustomTerrain>();
        return terrain != null && terrain.heightMapImage != null;
    }

    [MenuItem("Terrain/Load Texture Heights")]
    public static void LoadTextureHeights()
    {
        CustomTerrain terrain = Selection.activeGameObject.GetComponent<CustomTerrain>();
        if (terrain.additiveLoadHeights)
        {
            terrain.LoadTextureAdditive();
        }
        else
        {
            terrain.LoadTexture();
        }
        Debug.Log("Texture heights loaded for " + terrain.name);
    }
}
