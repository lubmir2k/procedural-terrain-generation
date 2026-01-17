using UnityEngine;
using UnityEditor;

public static class TerrainEditorUtility
{
    [MenuItem("Terrain/Generate Random Heights")]
    public static void GenerateRandomHeights()
    {
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.RandomTerrain();
            Debug.Log("Random heights generated!");
        }
        else
        {
            Debug.LogWarning("No CustomTerrain component found in scene.");
        }
    }

    [MenuItem("Terrain/Reset Terrain")]
    public static void ResetTerrain()
    {
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.ResetTerrain();
            Debug.Log("Terrain reset!");
        }
        else
        {
            Debug.LogWarning("No CustomTerrain component found in scene.");
        }
    }

    [MenuItem("Terrain/Load Texture Heights")]
    public static void LoadTextureHeights()
    {
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            if (terrain.heightMapImage != null)
            {
                terrain.LoadTexture();
                Debug.Log("Texture heights loaded!");
            }
            else
            {
                Debug.LogWarning("No height map image assigned to CustomTerrain.");
            }
        }
        else
        {
            Debug.LogWarning("No CustomTerrain component found in scene.");
        }
    }
}