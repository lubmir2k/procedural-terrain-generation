using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility for creating multi-terrain setups with proper neighbor relationships.
/// </summary>
public class MultiTerrainSetup : EditorWindow
{
    private int gridSizeX = 2;
    private int gridSizeZ = 2;
    private int terrainResolution = 513;
    private int terrainSize = 500;
    private int terrainHeight = 100;

    [MenuItem("Tools/Terrain/Multi-Terrain Setup")]
    public static void ShowWindow()
    {
        GetWindow<MultiTerrainSetup>("Multi-Terrain Setup");
    }

    [MenuItem("Tools/Terrain/Quick Setup/Create 2x2 Terrain Grid")]
    public static void CreateQuick2x2Grid()
    {
        CreateTerrainGrid(2, 2, 513, 500, 100);
    }

    [MenuItem("Tools/Terrain/Quick Setup/Setup Neighbors")]
    public static void QuickSetupNeighbors()
    {
        SetupNeighborsStatic();
    }

    [MenuItem("Tools/Terrain/Quick Setup/Add TController")]
    public static void QuickAddTController()
    {
        AddTControllerStatic();
    }

    private static void CreateTerrainGrid(int gridX, int gridZ, int resolution, int size, int height)
    {
        string folderPath = "Assets/TerrainData";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "TerrainData");
        }

        GameObject terrainParent = new GameObject("Terrain Grid");
        Undo.RegisterCreatedObjectUndo(terrainParent, "Create Multi-Terrain Grid");

        Terrain[,] terrains = new Terrain[gridX, gridZ];

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                TerrainData terrainData = new TerrainData();
                terrainData.heightmapResolution = resolution;
                terrainData.size = new Vector3(size, height, size);

                string assetPath = $"{folderPath}/TerrainData_{x}_{z}.asset";
                AssetDatabase.CreateAsset(terrainData, assetPath);

                GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
                terrainGO.name = $"Terrain_{x}_{z}";
                terrainGO.transform.parent = terrainParent.transform;
                terrainGO.transform.position = new Vector3(x * size, 0, z * size);

                terrainGO.tag = "Terrain";
                int terrainLayer = LayerMask.NameToLayer("Terrain");
                if (terrainLayer >= 0) terrainGO.layer = terrainLayer;

                terrains[x, z] = terrainGO.GetComponent<Terrain>();
            }
        }

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Terrain left = x > 0 ? terrains[x - 1, z] : null;
                Terrain right = x < gridX - 1 ? terrains[x + 1, z] : null;
                Terrain bottom = z > 0 ? terrains[x, z - 1] : null;
                Terrain top = z < gridZ - 1 ? terrains[x, z + 1] : null;

                terrains[x, z].SetNeighbors(left, top, right, bottom);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created {gridX}x{gridZ} terrain grid with neighbor relationships.");
    }

    private static void SetupNeighborsStatic()
    {
        Terrain[] allTerrains = Terrain.activeTerrains;
        if (allTerrains.Length == 0)
        {
            Debug.LogWarning("No active terrains found in the scene.");
            return;
        }

        float terrainWidth = allTerrains[0].terrainData.size.x;
        float terrainDepth = allTerrains[0].terrainData.size.z;

        var terrainGrid = new System.Collections.Generic.Dictionary<Vector2Int, Terrain>();

        foreach (Terrain t in allTerrains)
        {
            int gridX = Mathf.RoundToInt(t.transform.position.x / terrainWidth);
            int gridZ = Mathf.RoundToInt(t.transform.position.z / terrainDepth);
            terrainGrid[new Vector2Int(gridX, gridZ)] = t;
        }

        foreach (var kvp in terrainGrid)
        {
            Vector2Int pos = kvp.Key;
            Terrain terrain = kvp.Value;

            terrainGrid.TryGetValue(new Vector2Int(pos.x - 1, pos.y), out Terrain left);
            terrainGrid.TryGetValue(new Vector2Int(pos.x + 1, pos.y), out Terrain right);
            terrainGrid.TryGetValue(new Vector2Int(pos.x, pos.y - 1), out Terrain bottom);
            terrainGrid.TryGetValue(new Vector2Int(pos.x, pos.y + 1), out Terrain top);

            terrain.SetNeighbors(left, top, right, bottom);
        }

        Debug.Log($"Set up neighbor relationships for {allTerrains.Length} terrains.");
    }

    private static void AddTControllerStatic()
    {
        TController existing = Object.FindObjectOfType<TController>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("TController already exists in scene. Selected it.");
            return;
        }

        GameObject controllerGO = new GameObject("TerrainController");
        controllerGO.AddComponent<TController>();
        Undo.RegisterCreatedObjectUndo(controllerGO, "Add TController");

        Selection.activeGameObject = controllerGO;
        Debug.Log("Added TerrainController with TController component to the scene.");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Multi-Terrain Grid Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        gridSizeX = EditorGUILayout.IntSlider("Grid Width (X)", gridSizeX, 1, 4);
        gridSizeZ = EditorGUILayout.IntSlider("Grid Depth (Z)", gridSizeZ, 1, 4);

        EditorGUILayout.Space();

        terrainResolution = EditorGUILayout.IntPopup("Heightmap Resolution",
            terrainResolution,
            new string[] { "33", "65", "129", "257", "513", "1025" },
            new int[] { 33, 65, 129, 257, 513, 1025 });

        terrainSize = EditorGUILayout.IntField("Terrain Size (units)", terrainSize);
        terrainHeight = EditorGUILayout.IntField("Terrain Height (units)", terrainHeight);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            $"This will create a {gridSizeX}x{gridSizeZ} grid of terrains.\n" +
            $"Total size: {gridSizeX * terrainSize}x{gridSizeZ * terrainSize} units\n" +
            "Each terrain will have its own TerrainData asset.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Multi-Terrain Grid", GUILayout.Height(30)))
        {
            CreateMultiTerrainGrid();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup Neighbors for Existing Terrains"))
        {
            SetupNeighborsForExistingTerrains();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add TController to Scene"))
        {
            AddTControllerToScene();
        }
    }

    private void CreateMultiTerrainGrid()
    {
        // Create folder for terrain data if it doesn't exist
        string folderPath = "Assets/TerrainData";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "TerrainData");
        }

        // Create parent object for organization
        GameObject terrainParent = new GameObject("Terrain Grid");
        Undo.RegisterCreatedObjectUndo(terrainParent, "Create Multi-Terrain Grid");

        Terrain[,] terrains = new Terrain[gridSizeX, gridSizeZ];

        // Create terrain tiles
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Create unique TerrainData asset
                TerrainData terrainData = new TerrainData();
                terrainData.heightmapResolution = terrainResolution;
                terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);

                string assetPath = $"{folderPath}/TerrainData_{x}_{z}.asset";
                AssetDatabase.CreateAsset(terrainData, assetPath);

                // Create terrain GameObject
                GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
                terrainGO.name = $"Terrain_{x}_{z}";
                terrainGO.transform.parent = terrainParent.transform;
                terrainGO.transform.position = new Vector3(x * terrainSize, 0, z * terrainSize);

                // Set tag and layer
                terrainGO.tag = "Terrain";
                terrainGO.layer = LayerMask.NameToLayer("Terrain");

                terrains[x, z] = terrainGO.GetComponent<Terrain>();
            }
        }

        // Set up neighbor relationships
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Terrain left = x > 0 ? terrains[x - 1, z] : null;
                Terrain right = x < gridSizeX - 1 ? terrains[x + 1, z] : null;
                Terrain bottom = z > 0 ? terrains[x, z - 1] : null;
                Terrain top = z < gridSizeZ - 1 ? terrains[x, z + 1] : null;

                terrains[x, z].SetNeighbors(left, top, right, bottom);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created {gridSizeX}x{gridSizeZ} terrain grid with neighbor relationships.");
    }

    private void SetupNeighborsForExistingTerrains()
    {
        Terrain[] allTerrains = Terrain.activeTerrains;

        if (allTerrains.Length == 0)
        {
            EditorUtility.DisplayDialog("No Terrains", "No active terrains found in the scene.", "OK");
            return;
        }

        // Get terrain size from first terrain
        float terrainWidth = allTerrains[0].terrainData.size.x;
        float terrainDepth = allTerrains[0].terrainData.size.z;

        // Build a dictionary of terrains by their grid position
        System.Collections.Generic.Dictionary<Vector2Int, Terrain> terrainGrid =
            new System.Collections.Generic.Dictionary<Vector2Int, Terrain>();

        foreach (Terrain t in allTerrains)
        {
            int gridX = Mathf.RoundToInt(t.transform.position.x / terrainWidth);
            int gridZ = Mathf.RoundToInt(t.transform.position.z / terrainDepth);
            terrainGrid[new Vector2Int(gridX, gridZ)] = t;
        }

        // Set up neighbors
        foreach (var kvp in terrainGrid)
        {
            Vector2Int pos = kvp.Key;
            Terrain terrain = kvp.Value;

            terrainGrid.TryGetValue(new Vector2Int(pos.x - 1, pos.y), out Terrain left);
            terrainGrid.TryGetValue(new Vector2Int(pos.x + 1, pos.y), out Terrain right);
            terrainGrid.TryGetValue(new Vector2Int(pos.x, pos.y - 1), out Terrain bottom);
            terrainGrid.TryGetValue(new Vector2Int(pos.x, pos.y + 1), out Terrain top);

            terrain.SetNeighbors(left, top, right, bottom);
        }

        Debug.Log($"Set up neighbor relationships for {allTerrains.Length} terrains.");
    }

    private void AddTControllerToScene()
    {
        // Check if TController already exists
        TController existing = FindObjectOfType<TController>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("TController Exists",
                "A TController already exists in the scene. Selected it.", "OK");
            return;
        }

        GameObject controllerGO = new GameObject("TerrainController");
        TController controller = controllerGO.AddComponent<TController>();
        Undo.RegisterCreatedObjectUndo(controllerGO, "Add TController");

        Selection.activeGameObject = controllerGO;
        Debug.Log("Added TerrainController with TController component to the scene.");
    }
}
