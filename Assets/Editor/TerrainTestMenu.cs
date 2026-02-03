using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

public static class TerrainTestMenu
{
    private static bool TryGetTerrain(out CustomTerrain terrain)
    {
        terrain = UnityEngine.Object.FindFirstObjectByType<CustomTerrain>();
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

    [MenuItem("Tools/Terrain Test/Apply Vegetation")]
    public static void ApplyVegetation()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.PlantVegetation();
            Debug.Log($"Vegetation applied with MaxTrees={terrain.maxTrees}, TreeSpacing={terrain.treeSpacing}, VegetationTypes={terrain.vegetation.Count}");
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

    [MenuItem("Tools/Terrain Test/Generate Particle Clouds")]
    public static void GenerateParticleClouds()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.GenerateParticleClouds();
            Debug.Log($"Particle clouds generated: NumClouds={terrain.particleCloudData.numClouds}, ScaleMin={terrain.particleCloudData.cloudScaleMin}, ScaleMax={terrain.particleCloudData.cloudScaleMax}");
        }
    }

    [MenuItem("Tools/Terrain Test/Remove Particle Clouds")]
    public static void RemoveParticleClouds()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.RemoveParticleClouds();
            Debug.Log("Particle clouds removed.");
        }
    }

    [MenuItem("Tools/Terrain Test/Generate Rain")]
    public static void GenerateRain()
    {
        if (TryGetTerrain(out var terrain))
        {
            terrain.GenerateRain();
            Debug.Log($"Rain generated: MaxParticles={terrain.rainData.maxParticles}, EmissionRate={terrain.rainData.emissionRate}, WindForce={terrain.rainData.windForce}");
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

    // ==================== E2E WORKFLOW TESTS ====================

    [MenuItem("Tools/Terrain Test/Full Workflow Demo")]
    public static void FullWorkflowDemo()
    {
        if (!TryGetTerrain(out var terrain))
            return;

        var stopwatch = Stopwatch.StartNew();
        var results = new StringBuilder();
        int passed = 0;
        int failed = 0;

        Debug.Log("=== FULL WORKFLOW DEMO STARTED ===");

        // Step 1: Reset
        if (TryExecuteStep("Reset", 1, 6, () => terrain.ResetTerrain(), results))
            passed++;
        else
            failed++;

        // Step 2: Generate heights (MPD)
        if (TryExecuteStep("MPD Generation", 2, 6, () => terrain.MidpointDisplacement(), results))
            passed++;
        else
            failed++;

        // Step 3: Smooth
        if (TryExecuteStep("Smooth", 3, 6, () => terrain.Smooth(), results))
            passed++;
        else
            failed++;

        // Step 4: Splat Maps
        if (TryExecuteStep("Splat Maps", 4, 6, () => terrain.SplatMaps(), results))
            passed++;
        else
            failed++;

        // Step 5: Vegetation
        if (TryExecuteStep("Vegetation", 5, 6, () => terrain.PlantVegetation(), results))
            passed++;
        else
            failed++;

        // Step 6: Details
        if (TryExecuteStep("Details", 6, 6, () =>
        {
            terrain.AddDetails();
            terrain.terrain.detailObjectDistance = terrain.detailObjectDistance;
        }, results))
            passed++;
        else
            failed++;

        stopwatch.Stop();

        // Summary
        Debug.Log("\n=== FULL WORKFLOW SUMMARY ===");
        AppendVerificationSummary(terrain, results);
        Debug.Log(results.ToString());
        Debug.Log($"Steps: {passed} passed, {failed} failed");
        Debug.Log($"Time: {stopwatch.ElapsedMilliseconds}ms");
        Debug.Log("=== FULL WORKFLOW COMPLETE ===");

        // Show completion dialog
        EditorUtility.DisplayDialog(
            "Full Workflow Demo",
            $"Workflow completed!\n\n{passed} passed, {failed} failed\nTime: {stopwatch.ElapsedMilliseconds}ms\n\nSee Console for details.",
            "OK");
    }

    [MenuItem("Tools/Terrain Test/Verify Trees")]
    public static void VerifyTrees()
    {
        if (!TryGetTerrain(out var terrain))
            return;

        var terrainData = terrain.terrain.terrainData;
        var treeInstances = terrainData.treeInstances;
        var treePrototypes = terrainData.treePrototypes;

        Debug.Log("=== TREE VERIFICATION ===");
        Debug.Log($"Total tree instances: {treeInstances.Length}");
        Debug.Log($"Tree prototypes: {treePrototypes.Length}");

        if (treePrototypes.Length > 0)
        {
            // Count trees per prototype
            var counts = new int[treePrototypes.Length];
            foreach (var tree in treeInstances)
            {
                if (tree.prototypeIndex >= 0 && tree.prototypeIndex < counts.Length)
                    counts[tree.prototypeIndex]++;
            }

            for (int i = 0; i < treePrototypes.Length; i++)
            {
                string name = treePrototypes[i].prefab != null
                    ? treePrototypes[i].prefab.name
                    : $"Prototype {i}";
                Debug.Log($"  [{i}] {name}: {counts[i]} instances");
            }
        }

        Debug.Log("=== END TREE VERIFICATION ===");
    }

    [MenuItem("Tools/Terrain Test/Verify Splat Maps")]
    public static void VerifySplatMaps()
    {
        if (!TryGetTerrain(out var terrain))
            return;

        var terrainData = terrain.terrain.terrainData;
        var layers = terrainData.terrainLayers;
        var alphamaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        Debug.Log("=== SPLAT MAP VERIFICATION ===");
        Debug.Log($"Terrain layers: {layers.Length}");
        Debug.Log($"Alphamap size: {terrainData.alphamapWidth}x{terrainData.alphamapHeight}");

        if (layers.Length > 0)
        {
            int totalPixels = terrainData.alphamapWidth * terrainData.alphamapHeight;

            for (int layer = 0; layer < layers.Length; layer++)
            {
                float totalWeight = 0f;
                int dominantPixels = 0;

                for (int y = 0; y < terrainData.alphamapHeight; y++)
                {
                    for (int x = 0; x < terrainData.alphamapWidth; x++)
                    {
                        float weight = alphamaps[y, x, layer];
                        totalWeight += weight;

                        // Check if this layer is dominant at this pixel
                        bool isDominant = true;
                        for (int other = 0; other < layers.Length; other++)
                        {
                            if (other != layer && alphamaps[y, x, other] > weight)
                            {
                                isDominant = false;
                                break;
                            }
                        }
                        if (isDominant && weight > 0.01f)
                            dominantPixels++;
                    }
                }

                float avgWeight = totalWeight / totalPixels * 100f;
                float dominantPercent = (float)dominantPixels / totalPixels * 100f;
                string layerName = layers[layer] != null && layers[layer].diffuseTexture != null
                    ? layers[layer].diffuseTexture.name
                    : $"Layer {layer}";

                Debug.Log($"  [{layer}] {layerName}: avg weight {avgWeight:F1}%, dominant {dominantPercent:F1}%");
            }
        }

        Debug.Log("=== END SPLAT MAP VERIFICATION ===");
    }

    [MenuItem("Tools/Terrain Test/Verify Details Count")]
    public static void VerifyDetailsCount()
    {
        if (!TryGetTerrain(out var terrain))
            return;

        var terrainData = terrain.terrain.terrainData;
        var detailPrototypes = terrainData.detailPrototypes;

        Debug.Log("=== DETAIL VERIFICATION ===");
        Debug.Log($"Detail prototypes: {detailPrototypes.Length}");
        Debug.Log($"Detail resolution: {terrainData.detailResolution}");

        long totalInstances = 0;

        for (int i = 0; i < detailPrototypes.Length; i++)
        {
            var detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);
            long layerCount = 0;

            for (int y = 0; y < terrainData.detailHeight; y++)
            {
                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    layerCount += detailLayer[y, x];
                }
            }

            string name;
            if (detailPrototypes[i].prototype != null)
                name = detailPrototypes[i].prototype.name;
            else if (detailPrototypes[i].prototypeTexture != null)
                name = detailPrototypes[i].prototypeTexture.name + " (billboard)";
            else
                name = $"Prototype {i}";

            Debug.Log($"  [{i}] {name}: {layerCount:N0} instances");
            totalInstances += layerCount;
        }

        Debug.Log($"Total detail instances: {totalInstances:N0}");
        Debug.Log("=== END DETAIL VERIFICATION ===");
    }

    private static bool TryExecuteStep(string name, int step, int total, Action action, StringBuilder results)
    {
        Debug.Log($"[{step}/{total}] {name}...");
        try
        {
            action();
            results.AppendLine($"[OK] {name}");
            return true;
        }
        catch (Exception ex)
        {
            results.AppendLine($"[FAIL] {name}: {ex.Message}");
            Debug.LogError($"Step '{name}' failed: {ex.Message}");
            return false;
        }
    }

    private static void AppendVerificationSummary(CustomTerrain terrain, StringBuilder results)
    {
        var terrainData = terrain.terrain.terrainData;

        // Splat layers count
        int layerCount = terrainData.terrainLayers?.Length ?? 0;
        results.AppendLine($"  Splat Maps: {layerCount} layers");

        // Tree count
        int treeCount = terrainData.treeInstances?.Length ?? 0;
        results.AppendLine($"  Vegetation: {treeCount:N0} trees");

        // Detail count (sum all layers)
        long detailCount = 0;
        int prototypeCount = terrainData.detailPrototypes?.Length ?? 0;
        for (int i = 0; i < prototypeCount; i++)
        {
            var layer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);
            for (int y = 0; y < terrainData.detailHeight; y++)
            {
                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    detailCount += layer[y, x];
                }
            }
        }
        results.AppendLine($"  Details: {detailCount:N0} instances ({prototypeCount} prototypes)");
    }
}
