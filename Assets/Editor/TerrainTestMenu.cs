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

    [MenuItem("Tools/Terrain Test/Smooth")]
    public static void SmoothTerrain()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.Smooth();
            Debug.Log($"Terrain smoothed with Amount={terrain.smoothAmount}");
        }
    }

    [MenuItem("Tools/Terrain Test/Apply Details")]
    public static void ApplyDetails()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.AddDetails();
            terrain.terrain.detailObjectDistance = terrain.detailObjectDistance;
            Debug.Log($"Details applied with DetailObjectDistance={terrain.detailObjectDistance}, DetailSpacing={terrain.detailSpacing}, DetailCount={terrain.details.Count}");
        }
    }

    [MenuItem("Tools/Terrain Test/Erode")]
    public static void ErodeTerrain()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.Erode();
            Debug.Log($"Erosion applied with Type={terrain.erosionType}, Strength={terrain.erosionStrength}, Amount={terrain.erosionAmount}");
        }
    }

    [MenuItem("Tools/Terrain Test/Add Water")]
    public static void AddWater()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.AddWater();
            Debug.Log($"Water added at height={terrain.waterHeight}");
        }
    }
}
