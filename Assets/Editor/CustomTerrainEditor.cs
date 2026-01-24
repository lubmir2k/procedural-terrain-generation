using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    // ---------------------------
    // Reset Terrain
    // ---------------------------
    SerializedProperty resetTerrain;

    // ---------------------------
    // Random Heights
    // ---------------------------
    SerializedProperty randomHeightRange;
    bool showRandom = false;

    // ---------------------------
    // Load Heights from Texture
    // ---------------------------
    SerializedProperty heightMapImage;
    SerializedProperty heightMapScale;
    SerializedProperty additiveLoadHeights;
    bool showLoadHeights = false;

    // ---------------------------
    // Perlin Noise
    // ---------------------------
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistence;
    SerializedProperty perlinHeightScale;
    bool showPerlin = false;

    // ---------------------------
    // Multiple Perlin Noise
    // ---------------------------
    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;
    bool showMultiplePerlin = false;


    // ---------------------------
    // Voronoi Tessellation
    // ---------------------------
    SerializedProperty voronoiFalloff;
    SerializedProperty voronoiDropoff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiPeaks;
    SerializedProperty voronoiType;
    bool showVoronoi = false;

    // ---------------------------
    // Midpoint Displacement
    // ---------------------------
    SerializedProperty MPDheightMin;
    SerializedProperty MPDheightMax;
    SerializedProperty MPDheightDampenerPower;
    SerializedProperty MPDroughness;
    bool showMPD = false;

    // ---------------------------
    // Smooth
    // ---------------------------
    SerializedProperty smoothAmount;
    bool showSmooth = false;

    // ---------------------------
    // Splatmaps
    // ---------------------------
    GUITableState splatMapTable;
    SerializedProperty splatHeights;
    bool showSplatMaps = false;

    // ---------------------------
    // Vegetation
    // ---------------------------
    GUITableState vegMapTable;
    SerializedProperty vegetation;
    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;
    bool showVegetation = false;

    // ---------------------------
    // Details
    // ---------------------------
    GUITableState detailMapTable;
    SerializedProperty details;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;
    bool showDetail = false;

    // ---------------------------
    // Scroll View
    // ---------------------------
    Vector2 scrollPos;

    void OnEnable()
    {
        // Link serialized properties to the actual properties on our CustomTerrain
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        additiveLoadHeights = serializedObject.FindProperty("additiveLoadHeights");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistence = serializedObject.FindProperty("perlinPersistence");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        voronoiFalloff = serializedObject.FindProperty("voronoiFalloff");
        voronoiDropoff = serializedObject.FindProperty("voronoiDropoff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiPeaks = serializedObject.FindProperty("voronoiPeaks");
        voronoiType = serializedObject.FindProperty("voronoiType");
        MPDheightMin = serializedObject.FindProperty("MPDheightMin");
        MPDheightMax = serializedObject.FindProperty("MPDheightMax");
        MPDheightDampenerPower = serializedObject.FindProperty("MPDheightDampenerPower");
        MPDroughness = serializedObject.FindProperty("MPDroughness");
        smoothAmount = serializedObject.FindProperty("smoothAmount");
        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        vegMapTable = new GUITableState("vegMapTable");
        vegetation = serializedObject.FindProperty("vegetation");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        detailMapTable = new GUITableState("detailMapTable");
        details = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
    }

    public override void OnInspectorGUI()
    {
        // Always call this at the start to sync the serialized object
        serializedObject.Update();

        // Get reference to the terrain component
        CustomTerrain terrain = (CustomTerrain)target;

        // ---------------------------
        // Scrollbar Setup
        // ---------------------------
        // Let GUILayout handle sizing automatically (Rect dimensions are only valid during Repaint)
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUI.indentLevel++;

        // ---------------------------
        // Reset Terrain Toggle (at top for easy access)
        // ---------------------------
        EditorGUILayout.PropertyField(resetTerrain);

        // ---------------------------
        // Random Heights Section
        // ---------------------------
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            // Add a horizontal line separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Section label
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);

            // Property field for the Vector2 range
            EditorGUILayout.PropertyField(randomHeightRange);

            // Button to trigger the random terrain generation
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        // ---------------------------
        // Load Heights Section
        // ---------------------------
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            EditorGUILayout.PropertyField(additiveLoadHeights, 
                new GUIContent("Additive", "Add texture heights to existing terrain heights"));

            if (GUILayout.Button("Load Texture"))
            {
                // Use boolValue to get current GUI state (not stale object value)
                if (additiveLoadHeights.boolValue)
                {
                    terrain.LoadTextureAdditive();
                }
                else
                {
                    terrain.LoadTexture();
                }
            }
        }

        // ---------------------------
        // Perlin Noise Section
        // ---------------------------
        showPerlin = EditorGUILayout.Foldout(showPerlin, "Single Perlin Noise");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Perlin Noise Parameters", EditorStyles.boldLabel);

            // Sliders for scale values (small values 0-1 for smooth noise)
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("Y Scale"));

            // Int sliders for offset (seed) values
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y Offset"));

            // fBM parameters
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistence, 0.1f, 1f, new GUIContent("Persistence"));
            EditorGUILayout.Slider(perlinHeightScale, 0f, 1f, new GUIContent("Height Scale"));

            if (GUILayout.Button("Generate Perlin"))
            {
                terrain.Perlin();
            }
        }

        // ---------------------------
        // Multiple Perlin Noise Section
        // ---------------------------
        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);

            // Draw table using GUITable
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, perlinParameters);
            GUILayout.Space(20); // Prevent overlap with elements below

            // Add/Remove buttons in horizontal layout
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Apply Ridge Noise"))
            {
                terrain.RidgeNoise();
            }
        }

        // ---------------------------
        // Voronoi Tessellation Section
        // ---------------------------
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Voronoi Tessellation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(voronoiPeaks, 1, 12, new GUIContent("Peak Count"));
            EditorGUILayout.PropertyField(voronoiType);
            EditorGUILayout.Slider(voronoiFalloff, 0.01f, 10f, new GUIContent("Falloff"));
            EditorGUILayout.Slider(voronoiDropoff, 0f, 10f, new GUIContent("Drop Off"));
            EditorGUILayout.Slider(voronoiMinHeight, 0f, 1f, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0f, 1f, new GUIContent("Max Height"));

            if (GUILayout.Button("Generate Voronoi"))
            {
                terrain.Voronoi();
            }
        }

        // ---------------------------
        // Midpoint Displacement Section
        // ---------------------------
        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Midpoint Displacement (Diamond-Square)", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(MPDheightMin);
            EditorGUILayout.PropertyField(MPDheightMax);
            EditorGUILayout.PropertyField(MPDheightDampenerPower);
            EditorGUILayout.PropertyField(MPDroughness);

            if (GUILayout.Button("Generate MPD"))
            {
                terrain.MidpointDisplacement();
            }
        }

        // ---------------------------
        // Smooth Section
        // ---------------------------
        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth");
        if (showSmooth)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Smooth Terrain", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(smoothAmount, 1, 10, new GUIContent("Smooth Amount"));

            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        // ---------------------------
        // Splatmaps Section
        // ---------------------------
        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);

            splatMapTable = GUITableLayout.DrawTable(splatMapTable, splatHeights);

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Splat Maps"))
            {
                terrain.SplatMaps();
            }
        }

        // ---------------------------
        // Vegetation Section
        // ---------------------------
        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxTrees, 0, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 2, 20, new GUIContent("Tree Spacing"));

            vegMapTable = GUITableLayout.DrawTable(vegMapTable, serializedObject.FindProperty("vegetation"));

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewVegetation();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Vegetation"))
            {
                terrain.PlantVegetation();
            }
        }

        // ---------------------------
        // Details Section
        // ---------------------------
        showDetail = EditorGUILayout.Foldout(showDetail, "Details");
        if (showDetail)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Details (Grass/Rocks)", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Max Detail Distance"));
            EditorGUILayout.IntSlider(detailSpacing, 2, 20, new GUIContent("Detail Spacing"));

            detailMapTable = GUITableLayout.DrawTable(detailMapTable, details);

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewDetail();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveDetail();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Details"))
            {
                terrain.AddDetails();
                // Set detail object distance on the terrain component
                terrain.terrain.detailObjectDistance = maxDetails.intValue;
            }
        }

        // ---------------------------
        // Reset Section
        // ---------------------------
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        // ---------------------------
        // End Scrollbar
        // ---------------------------
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Always call this at the end to apply any changes
        serializedObject.ApplyModifiedProperties();
    }
}