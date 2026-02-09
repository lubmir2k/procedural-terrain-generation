using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility for creating multi-terrain setups with proper neighbor relationships.
/// </summary>
public class MultiTerrainSetup : EditorWindow
{
    // Constants for paths and names
    private const string TerrainDataFolderPath = "Assets/TerrainData";
    private const string TerrainDataFolderName = "TerrainData";
    private const string TerrainGridParentName = "Terrain Grid";

    // Constants for quick setup defaults
    private const int QuickGridSize = 2;
    private const int QuickTerrainResolution = 513;
    private const int QuickTerrainSize = 500;
    private const int QuickTerrainHeight = 100;

    // Instance fields for EditorWindow
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
        CreateTerrainGrid(QuickGridSize, QuickGridSize, QuickTerrainResolution, QuickTerrainSize, QuickTerrainHeight);
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
        if (!AssetDatabase.IsValidFolder(TerrainDataFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", TerrainDataFolderName);
        }

        GameObject terrainParent = new GameObject(TerrainGridParentName);
        Undo.RegisterCreatedObjectUndo(terrainParent, "Create Multi-Terrain Grid");

        Terrain[,] terrains = new Terrain[gridX, gridZ];

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                TerrainData terrainData = new TerrainData();
                terrainData.heightmapResolution = resolution;
                terrainData.size = new Vector3(size, height, size);

                string assetPath = $"{TerrainDataFolderPath}/TerrainData_{x}_{z}.asset";
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
        // Delegate to static method with instance parameters
        CreateTerrainGrid(gridSizeX, gridSizeZ, terrainResolution, terrainSize, terrainHeight);
    }

    private void SetupNeighborsForExistingTerrains()
    {
        // Check for empty with dialog feedback (EditorWindow context)
        if (Terrain.activeTerrains.Length == 0)
        {
            EditorUtility.DisplayDialog("No Terrains", "No active terrains found in the scene.", "OK");
            return;
        }

        // Delegate to static method for core logic
        SetupNeighborsStatic();
    }

    private void AddTControllerToScene()
    {
        // Check for existing with dialog feedback (EditorWindow context)
        TController existing = Object.FindObjectOfType<TController>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("TController Exists",
                "A TController already exists in the scene. Selected it.", "OK");
            return;
        }

        // Delegate to static method for core logic
        AddTControllerStatic();
    }
}
