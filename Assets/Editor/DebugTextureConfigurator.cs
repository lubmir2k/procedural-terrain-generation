using UnityEngine;
using UnityEditor;

/// <summary>
/// Replaces splat textures with 3 solid color swatches for debugging splat map placement.
/// Non-overlapping height/slope ranges with no noise for clean visualization.
/// Green=low flat, Gray=high flat, Brown=steep slopes.
/// </summary>
public static class DebugTextureConfigurator
{
    private const string DEBUG_PATH = "Assets/Textures/DebugColors/";

    private struct SplatConfig
    {
        public string texturePath;
        public float minHeight;
        public float maxHeight;
        public float minSlope;
        public float maxSlope;
        public Vector2 tileSize;
        public float splatOffset;
    }

    private static readonly SplatConfig[] DebugBiome = new SplatConfig[]
    {
        // Layer 0: Green - Low flat ground
        new SplatConfig
        {
            texturePath = DEBUG_PATH + "Green 1.psd",
            minHeight = 0.0f,
            maxHeight = 0.33f,
            minSlope = 0f,
            maxSlope = 30f,
            tileSize = new Vector2(10, 10),
            splatOffset = 0.01f
        },
        // Layer 1: Gray - High flat ground
        new SplatConfig
        {
            texturePath = DEBUG_PATH + "Gray 1.psd",
            minHeight = 0.33f,
            maxHeight = 1.0f,
            minSlope = 0f,
            maxSlope = 30f,
            tileSize = new Vector2(10, 10),
            splatOffset = 0.01f
        },
        // Layer 2: Brown - Steep slopes (any height)
        new SplatConfig
        {
            texturePath = DEBUG_PATH + "Brown 1.psd",
            minHeight = 0.0f,
            maxHeight = 1.0f,
            minSlope = 30f,
            maxSlope = 90f,
            tileSize = new Vector2(10, 10),
            splatOffset = 0.01f
        }
    };

    [MenuItem("Tools/Biome/Configure Debug Solid Colors")]
    public static void ConfigureDebugColors()
    {
        var terrains = Object.FindObjectsByType<CustomTerrain>(FindObjectsSortMode.None);
        if (terrains.Length == 0)
        {
            Debug.LogError("No CustomTerrain components found in the scene.");
            return;
        }

        int terrainCount = 0;
        foreach (var terrain in terrains)
        {
            terrain.splatHeights.Clear();

            int configured = 0;
            foreach (var config in DebugBiome)
            {
                Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(config.texturePath);
                if (albedo == null)
                {
                    Debug.LogWarning($"Debug texture not found: {config.texturePath}");
                    continue;
                }

                var splat = new CustomTerrain.SplatHeights
                {
                    texture = albedo,
                    textureNormalMap = null,
                    minHeight = config.minHeight,
                    maxHeight = config.maxHeight,
                    minSlope = config.minSlope,
                    maxSlope = config.maxSlope,
                    tileOffset = Vector2.zero,
                    tileSize = config.tileSize,
                    splatOffset = config.splatOffset,
                    splatNoiseXScale = 0f,
                    splatNoiseYScale = 0f,
                    splatNoiseScaler = 0f,
                    remove = false
                };

                terrain.splatHeights.Add(splat);
                configured++;
            }

            EditorUtility.SetDirty(terrain);
            terrainCount++;
            Debug.Log($"Configured {configured} debug color layers on '{terrain.gameObject.name}'.");
        }

        Debug.Log($"Debug solid colors configured on {terrainCount} terrain(s). Green=low flat, Gray=high flat, Brown=steep slopes.");
        Debug.Log("Run 'Tools > Terrain Test > Apply Splat Maps' to apply.");
    }
}