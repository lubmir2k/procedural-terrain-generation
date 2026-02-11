using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Multi-terrain controller that applies seamless Perlin noise across all active terrains.
/// Attach this to an empty GameObject in the scene (e.g., "TerrainController").
/// Create neighboring terrains using Unity's terrain neighbor tool, then adjust parameters
/// in the inspector to generate seamless terrain across all tiles.
/// </summary>
[ExecuteInEditMode]
public class TController : MonoBehaviour
{
    // ---------------------------
    // Constants
    // ---------------------------
    /// <summary>
    /// Additional offset to ensure noise sampling starts well away from world origin.
    /// Prevents mirroring artifacts that can occur when sampling near (0,0).
    /// </summary>
    private const float WorldOriginOffset = 1000f;

    // ---------------------------
    // Perlin Noise Parameters
    // ---------------------------
    [Header("Perlin Noise Settings")]
    [Tooltip("X scale for Perlin noise sampling")]
    public float perlinXScale = 0.01f;

    [Tooltip("Z scale for Perlin noise sampling")]
    public float perlinZScale = 0.01f;

    [Tooltip("X offset for Perlin noise sampling")]
    public float perlinOffsetX = 0f;

    [Tooltip("Z offset for Perlin noise sampling")]
    public float perlinOffsetZ = 0f;

    [Tooltip("Height multiplier for the generated terrain")]
    [Range(0f, 1f)]
    public float perlinHeightScale = 0.1f;

    [Tooltip("Persistence controls amplitude decay per octave (typically 0.5)")]
    [Range(0.1f, 1f)]
    public float perlinPersistence = 0.5f;

    [Tooltip("Number of noise octaves to combine")]
    [Range(1, 10)]
    public int perlinOctaves = 4;

    [Header("Mirroring Fix")]
    [Tooltip("Enable automatic offset to prevent mirroring artifacts at world origin")]
    public bool useZeroOffset = true;

    [Tooltip("Manual X offset added to world position before sampling (auto-calculated if useZeroOffset is true)")]
    public float perlinZeroOffsetX = 0f;

    [Tooltip("Manual Z offset added to world position before sampling (auto-calculated if useZeroOffset is true)")]
    public float perlinZeroOffsetZ = 0f;

    [Header("Seam Stitching")]
    [Tooltip("Enable edge stitching to guarantee seamless borders (handles floating-point precision)")]
    public bool enableSeamStitching = true;

    // Cache to prevent excessive regeneration
    private bool _isGenerating = false;

#if UNITY_EDITOR
    /// <summary>
    /// Called when any serialized property changes in the inspector.
    /// Uses EditorApplication.delayCall to avoid errors during domain reload.
    /// </summary>
    private void OnValidate()
    {
        if (_isGenerating) return;

        // Use delayCall to avoid errors when called during serialization
        EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Object may have been destroyed
            GenerateAllTerrains();
        };
    }
#endif

    /// <summary>
    /// Generates seamless Perlin noise across all active terrains in the scene.
    /// </summary>
    public void GenerateAllTerrains()
    {
        if (_isGenerating) return;
        _isGenerating = true;

        try
        {
            Terrain[] terrains = Terrain.activeTerrains;

            if (terrains == null || terrains.Length == 0)
            {
                Debug.LogWarning("No active terrains found in the scene.", this);
                return;
            }

            // Calculate dynamic zero offset to prevent mirroring at world origin
            float zeroOffsetX = perlinZeroOffsetX;
            float zeroOffsetZ = perlinZeroOffsetZ;

            if (useZeroOffset && terrains.Length > 0)
            {
                // Calculate actual world bounds of all terrains to determine proper offset
                // This handles non-square grids better than just using terrains.Length
                float minX = float.MaxValue, maxX = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;

                foreach (Terrain t in terrains)
                {
                    if (t == null || t.terrainData == null) continue;
                    Vector3 pos = t.transform.position;
                    Vector3 size = t.terrainData.size;

                    minX = Mathf.Min(minX, pos.x);
                    maxX = Mathf.Max(maxX, pos.x + size.x);
                    minZ = Mathf.Min(minZ, pos.z);
                    maxZ = Mathf.Max(maxZ, pos.z + size.z);
                }

                // Offset by total world size to ensure sampling starts well away from origin
                float worldSizeX = maxX - minX;
                float worldSizeZ = maxZ - minZ;
                zeroOffsetX = Mathf.Max(worldSizeX, worldSizeZ) + WorldOriginOffset;
                zeroOffsetZ = zeroOffsetX;
            }

            // First pass: Generate heights for all terrains using world position
            foreach (Terrain terrain in terrains)
            {
                if (terrain == null || terrain.terrainData == null) continue;

                GenerateTerrainHeights(terrain, zeroOffsetX, zeroOffsetZ);
            }

            // Second pass: Stitch seams between neighboring terrains (optional)
            if (enableSeamStitching)
            {
                StitchAllTerrains();
            }
        }
        finally
        {
            _isGenerating = false;
        }
    }

    /// <summary>
    /// Generates heightmap for a single terrain using world-space Perlin noise.
    /// </summary>
    /// <param name="terrain">The terrain to generate heights for</param>
    /// <param name="zeroOffsetX">X offset to prevent mirroring</param>
    /// <param name="zeroOffsetZ">Z offset to prevent mirroring</param>
    private void GenerateTerrainHeights(Terrain terrain, float zeroOffsetX, float zeroOffsetZ)
    {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        // Loop through every point in the heightmap
        // Note: heights[x, z] where x is the first index (row) and z is second (column)
        // In Unity's terrain system, the first index corresponds to Z-axis, second to X-axis
        // when used with SetHeights, but we calculate world position correctly below
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Calculate world position for this heightmap point
                // x/resolution gives normalized position (0-1), multiply by terrain size
                // then add terrain's world position for absolute world coordinates
                float worldPosX = (x / (float)(resolution - 1) * terrainSize.x) + terrainPos.x;
                float worldPosZ = (z / (float)(resolution - 1) * terrainSize.z) + terrainPos.z;

                // Apply offsets to prevent mirroring and allow user adjustment
                float sampleX = (worldPosX + zeroOffsetX + perlinOffsetX) * perlinXScale;
                float sampleZ = (worldPosZ + zeroOffsetZ + perlinOffsetZ) * perlinZScale;

                // Sample Perlin noise using existing FBM function
                float noiseValue = Utils.FBM(sampleX, sampleZ, perlinOctaves, perlinPersistence);

                // Apply height scale and clamp to valid range
                heights[z, x] = Mathf.Clamp01(noiseValue * perlinHeightScale);
            }
        }

        // Apply the heightmap to the terrain
        terrainData.SetHeights(0, 0, heights);
    }

    /// <summary>
    /// Stitches seams for all active terrains without regenerating heights.
    /// Can be used after loading heightmaps or any other height modification.
    /// </summary>
    public void StitchAllTerrains()
    {
        Terrain[] terrains = Terrain.activeTerrains;

        if (terrains == null || terrains.Length == 0)
        {
            Debug.LogWarning("No active terrains found in the scene.", this);
            return;
        }

        foreach (Terrain terrain in terrains)
        {
            if (terrain == null || terrain.terrainData == null) continue;
            StitchTerrainSeams(terrain);
        }
    }

    /// <summary>
    /// Stitches seams between this terrain and its neighbors by copying edge heights.
    /// Only handles top and right neighbors to avoid double-processing.
    /// </summary>
    /// <param name="terrain">The terrain to stitch</param>
    private void StitchTerrainSeams(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        // Stitch top neighbor (copy our top edge to neighbor's bottom edge)
        if (terrain.topNeighbor != null && terrain.topNeighbor.terrainData != null)
        {
            // Unity terrain height array convention: heights[z, x] or heights[row, column]
            // GetHeights(xBase, yBase, width, height) returns float[height, width] = float[z, x]
            // topEdge will be float[1, resolution] - 1 row (Z), all columns (X)
            float[,] topEdge = terrainData.GetHeights(0, resolution - 1, resolution, 1);

            // SetHeights(xBase, yBase, heights) expects heights[z, x]
            // Setting at (0, 0) places this row at the neighbor's bottom edge
            terrain.topNeighbor.terrainData.SetHeights(0, 0, topEdge);
        }

        // Stitch right neighbor (copy our right edge to neighbor's left edge)
        if (terrain.rightNeighbor != null && terrain.rightNeighbor.terrainData != null)
        {
            // Get the right edge of this terrain (last column)
            float[,] rightEdge = terrainData.GetHeights(resolution - 1, 0, 1, resolution);

            // Set as the left edge of the right neighbor (first column)
            terrain.rightNeighbor.terrainData.SetHeights(0, 0, rightEdge);
        }
    }

    /// <summary>
    /// Resets all active terrains to flat (height 0).
    /// </summary>
    public void ResetAllTerrains()
    {
        Terrain[] terrains = Terrain.activeTerrains;

        foreach (Terrain terrain in terrains)
        {
            if (terrain == null || terrain.terrainData == null) continue;

            TerrainData terrainData = terrain.terrainData;
            int resolution = terrainData.heightmapResolution;
            float[,] heights = new float[resolution, resolution];

            // Heights array is initialized to 0 by default
            terrainData.SetHeights(0, 0, heights);
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Menu items for TController operations.
/// </summary>
public static class TControllerMenu
{
    [MenuItem("Tools/Terrain/Generate All Terrains")]
    public static void GenerateAllTerrainsMenu()
    {
        TController controller = Object.FindAnyObjectByType<TController>();
        if (controller != null)
        {
            controller.GenerateAllTerrains();
            Debug.Log("Generated terrain for all active terrains.");
        }
        else
        {
            Debug.LogWarning("No TController found in the scene. Add one first.");
        }
    }

    [MenuItem("Tools/Terrain/Reset All Terrains")]
    public static void ResetAllTerrainsMenu()
    {
        TController controller = Object.FindAnyObjectByType<TController>();
        if (controller != null)
        {
            controller.ResetAllTerrains();
            Debug.Log("Reset all terrains to flat.");
        }
        else
        {
            Debug.LogWarning("No TController found in the scene. Add one first.");
        }
    }

    [MenuItem("Tools/Terrain/Stitch All Terrains")]
    public static void StitchAllTerrainsMenu()
    {
        TController controller = Object.FindAnyObjectByType<TController>();
        if (controller != null)
        {
            controller.StitchAllTerrains();
            Debug.Log("Stitched seams for all active terrains.");
        }
        else
        {
            Debug.LogWarning("No TController found in the scene. Add one first.");
        }
    }
}

[CustomEditor(typeof(TController))]
public class TControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TController controller = (TController)target;

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // Generate button
        if (GUILayout.Button("Generate All Terrains", GUILayout.Height(30)))
        {
            controller.GenerateAllTerrains();
        }

        // Stitch button
        if (GUILayout.Button("Stitch All Terrains"))
        {
            controller.StitchAllTerrains();
        }

        // Reset button
        if (GUILayout.Button("Reset All Terrains"))
        {
            if (EditorUtility.DisplayDialog("Reset Terrains",
                "Are you sure you want to reset all terrains to flat?",
                "Yes", "Cancel"))
            {
                controller.ResetAllTerrains();
            }
        }

        EditorGUILayout.Space(5);

        // Info box
        EditorGUILayout.HelpBox(
            $"Active Terrains: {Terrain.activeTerrains?.Length ?? 0}\n" +
            "Use Unity's terrain neighbor tool to create adjacent terrains, " +
            "then adjust parameters above for seamless generation.",
            MessageType.Info);
    }
}
#endif
