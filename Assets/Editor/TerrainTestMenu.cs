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

    // ==================== MULTI-TERRAIN E2E TEST ====================

    private const int TestGridSize = 2;
    private const int TestResolution = 33;  // Minimal for fast tests
    private const int TestSize = 100;
    private const int TestHeight = 50;
    private const string TestFolder = "Assets/TerrainData/Test";
    private const string TestParentName = "TestTerrainGrid";
    private const float SeamTolerance = 0.0001f;

    [MenuItem("Tools/Terrain Test/Multi-Terrain E2E")]
    public static void MultiTerrainE2ETest()
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new StringBuilder();
        int passed = 0;
        int failed = 0;

        Debug.Log("=== MULTI-TERRAIN E2E TEST STARTED ===");

        // Track created assets for cleanup
        var createdAssets = new System.Collections.Generic.List<string>();
        GameObject testParent = null;
        Terrain[,] terrains = null;

        try
        {
            // Step 1: Create Grid
            bool gridCreated = TryExecuteStep("Create Grid", 1, 6, () =>
            {
                testParent = CreateTestTerrainGrid(createdAssets, out terrains);
            }, results);

            if (gridCreated)
            {
                passed++;

                // Step 2: Verify Positions
                if (TryExecuteStep("Verify Positions", 2, 6, () =>
                {
                    VerifyTerrainPositions(terrains);
                }, results))
                    passed++;
                else
                    failed++;

                // Step 3: Verify Neighbors
                if (TryExecuteStep("Verify Neighbors", 3, 6, () =>
                {
                    VerifyNeighborRelationships(terrains);
                }, results))
                    passed++;
                else
                    failed++;

                // Step 4: Generate Heights
                if (TryExecuteStep("Generate Heights", 4, 6, () =>
                {
                    TController controller = testParent.AddComponent<TController>();
                    controller.perlinXScale = 0.01f;
                    controller.perlinZScale = 0.01f;
                    controller.perlinHeightScale = 0.5f;
                    controller.perlinOctaves = 4;
                    controller.perlinPersistence = 0.5f;
                    controller.enableSeamStitching = true;
                    controller.GenerateAllTerrains();
                }, results))
                    passed++;
                else
                    failed++;

                // Step 5: Verify Heights
                if (TryExecuteStep("Verify Heights", 5, 6, () =>
                {
                    VerifyHeightsGenerated(terrains);
                }, results))
                    passed++;
                else
                    failed++;

                // Step 6: Verify Seams
                if (TryExecuteStep("Verify Seams", 6, 6, () =>
                {
                    VerifySeamStitching(terrains);
                }, results))
                    passed++;
                else
                    failed++;
            }
            else
            {
                failed++;
            }
        }
        finally
        {
            // Cleanup test objects
            CleanupTestTerrains(testParent, createdAssets);
        }

        stopwatch.Stop();

        // Summary
        Debug.Log("\n" + results.ToString());
        string status = failed == 0 ? "PASSED" : "FAILED";
        Debug.Log($"=== MULTI-TERRAIN E2E TEST {status} ===");
        Debug.Log($"Steps: {passed} passed, {failed} failed");
        Debug.Log($"Time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private static GameObject CreateTestTerrainGrid(System.Collections.Generic.List<string> createdAssets, out Terrain[,] terrains)
    {
        // Create test folder if needed
        if (!AssetDatabase.IsValidFolder(TestFolder))
        {
            AssetDatabase.CreateFolder("Assets/TerrainData", "Test");
        }

        GameObject testParent = new GameObject(TestParentName);
        terrains = new Terrain[TestGridSize, TestGridSize];

        for (int x = 0; x < TestGridSize; x++)
        {
            for (int z = 0; z < TestGridSize; z++)
            {
                TerrainData terrainData = new TerrainData();
                terrainData.heightmapResolution = TestResolution;
                terrainData.size = new Vector3(TestSize, TestHeight, TestSize);

                string assetPath = $"{TestFolder}/TestTerrainData_{x}_{z}.asset";
                AssetDatabase.CreateAsset(terrainData, assetPath);
                createdAssets.Add(assetPath);

                GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
                terrainGO.name = $"TestTerrain_{x}_{z}";
                terrainGO.transform.parent = testParent.transform;
                terrainGO.transform.position = new Vector3(x * TestSize, 0, z * TestSize);

                terrains[x, z] = terrainGO.GetComponent<Terrain>();
            }
        }

        // Set up neighbor relationships
        for (int x = 0; x < TestGridSize; x++)
        {
            for (int z = 0; z < TestGridSize; z++)
            {
                Terrain left = x > 0 ? terrains[x - 1, z] : null;
                Terrain right = x < TestGridSize - 1 ? terrains[x + 1, z] : null;
                Terrain bottom = z > 0 ? terrains[x, z - 1] : null;
                Terrain top = z < TestGridSize - 1 ? terrains[x, z + 1] : null;

                terrains[x, z].SetNeighbors(left, top, right, bottom);
            }
        }

        AssetDatabase.SaveAssets();
        return testParent;
    }

    private static void VerifyTerrainPositions(Terrain[,] terrains)
    {
        for (int x = 0; x < TestGridSize; x++)
        {
            for (int z = 0; z < TestGridSize; z++)
            {
                Vector3 actual = terrains[x, z].transform.position;
                Vector3 expected = new Vector3(x * TestSize, 0, z * TestSize);

                if (actual != expected)
                {
                    throw new Exception($"Terrain [{x},{z}] at {actual} expected {expected}");
                }
            }
        }
    }

    private static void VerifyNeighborRelationships(Terrain[,] terrains)
    {
        for (int x = 0; x < TestGridSize; x++)
        {
            for (int z = 0; z < TestGridSize; z++)
            {
                Terrain current = terrains[x, z];
                Terrain expectedLeft = x > 0 ? terrains[x - 1, z] : null;
                Terrain expectedRight = x < TestGridSize - 1 ? terrains[x + 1, z] : null;
                Terrain expectedBottom = z > 0 ? terrains[x, z - 1] : null;
                Terrain expectedTop = z < TestGridSize - 1 ? terrains[x, z + 1] : null;

                if (current.leftNeighbor != expectedLeft)
                    throw new Exception($"Terrain[{x},{z}] has incorrect left neighbor.");
                if (current.rightNeighbor != expectedRight)
                    throw new Exception($"Terrain[{x},{z}] has incorrect right neighbor.");
                if (current.bottomNeighbor != expectedBottom)
                    throw new Exception($"Terrain[{x},{z}] has incorrect bottom neighbor.");
                if (current.topNeighbor != expectedTop)
                    throw new Exception($"Terrain[{x},{z}] has incorrect top neighbor.");
            }
        }
    }

    private static void VerifyHeightsGenerated(Terrain[,] terrains)
    {
        for (int x = 0; x < TestGridSize; x++)
        {
            for (int z = 0; z < TestGridSize; z++)
            {
                TerrainData td = terrains[x, z].terrainData;
                int res = td.heightmapResolution;
                float[,] heights = td.GetHeights(0, 0, res, res);

                bool hasNonZero = false;
                for (int hz = 0; hz < res && !hasNonZero; hz++)
                {
                    for (int hx = 0; hx < res && !hasNonZero; hx++)
                    {
                        float h = heights[hz, hx];
                        if (h < 0f || h > 1f)
                            throw new Exception($"Terrain[{x},{z}] has height {h} outside [0,1] at ({hx},{hz})");
                        if (h > 0.001f)
                            hasNonZero = true;
                    }
                }

                if (!hasNonZero)
                    throw new Exception($"Terrain[{x},{z}] has no non-zero heights");
            }
        }
    }

    private static void VerifySeamStitching(Terrain[,] terrains)
    {
        int res = TestResolution;
        int gridSize = terrains.GetLength(0);

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (x < gridSize - 1)
                {
                    VerifyHorizontalSeam(terrains[x, z], terrains[x + 1, z], res, $"Terrain[{x},{z}]->Terrain[{x + 1},{z}]");
                }

                if (z < gridSize - 1)
                {
                    VerifyVerticalSeam(terrains[x, z], terrains[x, z + 1], res, $"Terrain[{x},{z}]->Terrain[{x},{z + 1}]");
                }
            }
        }
    }

    private static void VerifyHorizontalSeam(Terrain left, Terrain right, int res, string seamName)
    {
        // Right edge of left terrain (last column)
        float[,] leftEdge = left.terrainData.GetHeights(res - 1, 0, 1, res);
        // Left edge of right terrain (first column)
        float[,] rightEdge = right.terrainData.GetHeights(0, 0, 1, res);

        for (int z = 0; z < res; z++)
        {
            float leftH = leftEdge[z, 0];
            float rightH = rightEdge[z, 0];
            if (Mathf.Abs(leftH - rightH) > SeamTolerance)
            {
                throw new Exception($"Seam mismatch at {seamName} row {z}: left={leftH}, right={rightH}");
            }
        }
    }

    private static void VerifyVerticalSeam(Terrain bottom, Terrain top, int res, string seamName)
    {
        // Top edge of bottom terrain (last row)
        float[,] bottomEdge = bottom.terrainData.GetHeights(0, res - 1, res, 1);
        // Bottom edge of top terrain (first row)
        float[,] topEdge = top.terrainData.GetHeights(0, 0, res, 1);

        for (int x = 0; x < res; x++)
        {
            float bottomH = bottomEdge[0, x];
            float topH = topEdge[0, x];
            if (Mathf.Abs(bottomH - topH) > SeamTolerance)
            {
                throw new Exception($"Seam mismatch at {seamName} col {x}: bottom={bottomH}, top={topH}");
            }
        }
    }

    private static void CleanupTestTerrains(GameObject testParent, System.Collections.Generic.List<string> createdAssets)
    {
        // Destroy test GameObjects
        if (testParent != null)
        {
            UnityEngine.Object.DestroyImmediate(testParent);
        }

        // Delete created assets (in reverse order to delete folder last)
        for (int i = createdAssets.Count - 1; i >= 0; i--)
        {
            string path = createdAssets[i];
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        // Try to delete the test folder if it exists and is empty
        if (AssetDatabase.IsValidFolder(TestFolder))
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        AssetDatabase.Refresh();
    }
}
