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

    [MenuItem("Tools/Terrain Test/Apply Splat Maps")]
    public static void ApplySplatMaps()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.SplatMaps();
            Debug.Log($"Splat Maps applied with {terrain.splatHeights.Count} texture layers");
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

    [MenuItem("Tools/Terrain Test/Apply Fog")]
    public static void ApplyFog()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.ApplyFog();
            Debug.Log($"Fog applied: Enabled={terrain.enableFog}, Mode={terrain.fogMode}, Color={terrain.fogColor}, Density={terrain.fogDensity}");
        }
    }

    [MenuItem("Tools/Terrain Test/Generate Clouds")]
    public static void GenerateClouds()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.GenerateClouds();
            Debug.Log($"Clouds generated ({terrain.cloudData.mode}): Height={terrain.cloudData.cloudHeight}, Scale={terrain.cloudData.cloudScale}");
        }
    }

    [MenuItem("Tools/Terrain Test/Remove Clouds")]
    public static void RemoveClouds()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.RemoveClouds();
            Debug.Log("Cloud plane removed.");
        }
    }

    [MenuItem("Tools/Terrain Test/Generate Rain")]
    public static void GenerateRain()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.GenerateRain();
            Debug.Log($"Rain generated: MaxParticles={terrain.rainData.maxParticles}, EmissionRate={terrain.rainData.emissionRate}");
        }
    }

    [MenuItem("Tools/Terrain Test/Remove Rain")]
    public static void RemoveRain()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.RemoveRain();
            Debug.Log("Rain removed.");
        }
    }
}
