using UnityEngine;
using UnityEditor;

public static class TerrainTestMenu
{
    private static bool TryGetTerrain(out CustomTerrain terrain)
    {
        terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain == null)
        {
            Debug.LogWarning("No CustomTerrain object found in the scene.");
            return false;
        }
        return true;
    }

    [MenuItem("Tools/Terrain Test/Reset")]
    public static void ResetTerrain()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.ResetTerrain();
            Debug.Log("Terrain reset complete.");
        }
    }

    [MenuItem("Tools/Terrain Test/Generate Voronoi")]
    public static void GenerateVoronoi()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.Voronoi();
            Debug.Log($"Voronoi generated with Type={terrain.voronoiType}, Peaks={terrain.voronoiPeaks}, Falloff={terrain.voronoiFalloff}, DropOff={terrain.voronoiDropoff}, MinHeight={terrain.voronoiMinHeight}, MaxHeight={terrain.voronoiMaxHeight}");
        }
    }

    [MenuItem("Tools/Terrain Test/Generate MPD")]
    public static void GenerateMPD()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.MidpointDisplacement();
            Debug.Log($"Midpoint Displacement generated with HeightMin={terrain.MPDheightMin}, HeightMax={terrain.MPDheightMax}, DampenerPower={terrain.MPDheightDampenerPower}, Roughness={terrain.MPDroughness}");
        }
    }
}
