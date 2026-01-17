using UnityEngine;
using UnityEditor;

public static class TerrainTestMenu
{
    [MenuItem("Tools/Terrain Test/Reset")]
    public static void ResetTerrain()
    {
        var terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.ResetTerrain();
            Debug.Log("Terrain reset complete.");
        }
    }

    [MenuItem("Tools/Terrain Test/Generate Voronoi")]
    public static void GenerateVoronoi()
    {
        var terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.Voronoi();
            Debug.Log($"Voronoi generated with Peaks={terrain.voronoiPeaks}, Falloff={terrain.voronoiFalloff}, DropOff={terrain.voronoiDropoff}");
        }
    }
}
