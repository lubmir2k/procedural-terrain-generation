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
    SerializedProperty useRidgedNoise;
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
    SerializedProperty detailObjectDistance;
    SerializedProperty detailSpacing;
    bool showDetail = false;

    // ---------------------------
    // Water
    // ---------------------------
    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    bool showWater = false;

    // ---------------------------
    // Erosion
    // ---------------------------
    SerializedProperty erosionType;
    SerializedProperty erosionStrength;
    SerializedProperty erosionAmount;
    SerializedProperty droplets;
    SerializedProperty solubility;
    SerializedProperty springsPerRiver;
    SerializedProperty erosionSmoothAmount;
    SerializedProperty windDirection;
    bool showErosion = false;

    // ---------------------------
    // Fog
    // ---------------------------
    SerializedProperty enableFog;
    SerializedProperty fogMode;
    SerializedProperty fogColor;
    SerializedProperty fogDensity;
    SerializedProperty fogStartDistance;
    SerializedProperty fogEndDistance;
    bool showFog = false;

    // ---------------------------
    // Clouds
    // ---------------------------
    SerializedProperty cloudData;
    SerializedProperty cloudMode;
    SerializedProperty cloudMat;
    SerializedProperty skydomeMesh;
    SerializedProperty cloudHeight;
    SerializedProperty cloudScale;
    bool showClouds = false;

    // ---------------------------
    // Particle Clouds
    // ---------------------------
    SerializedProperty particleCloudData;
    SerializedProperty numClouds;
    SerializedProperty particlesPerCloud;
    SerializedProperty cloudParticleSize;
    SerializedProperty cloudScaleMin;
    SerializedProperty cloudScaleMax;
    SerializedProperty particleCloudMaterial;
    SerializedProperty particleCloudShadowMaterial;
    SerializedProperty particleCloudColor;
    SerializedProperty particleCloudLining;
    SerializedProperty cloudMinSpeed;
    SerializedProperty cloudMaxSpeed;
    SerializedProperty cloudRange;
    bool showParticleClouds = false;

    // ---------------------------
    // Rain
    // ---------------------------
    SerializedProperty rainData;
    SerializedProperty rainMaxParticles;
    SerializedProperty rainEmissionRate;
    SerializedProperty rainParticleLifetime;
    SerializedProperty rainStartSpeed;
    SerializedProperty rainStartSize;
    SerializedProperty rainColor;
    SerializedProperty rainGravityModifier;
    SerializedProperty rainEnableCollision;
    SerializedProperty rainEnableSplashes;
    SerializedProperty rainMaterial;
    SerializedProperty rainSplashMaterial;
    SerializedProperty rainWindForce;
    bool showRain = false;

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
        useRidgedNoise = serializedObject.FindProperty("useRidgedNoise");
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
        detailObjectDistance = serializedObject.FindProperty("detailObjectDistance");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        erosionType = serializedObject.FindProperty("erosionType");
        erosionStrength = serializedObject.FindProperty("erosionStrength");
        erosionAmount = serializedObject.FindProperty("erosionAmount");
        droplets = serializedObject.FindProperty("droplets");
        solubility = serializedObject.FindProperty("solubility");
        springsPerRiver = serializedObject.FindProperty("springsPerRiver");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");
        windDirection = serializedObject.FindProperty("windDirection");
        enableFog = serializedObject.FindProperty("enableFog");
        fogMode = serializedObject.FindProperty("fogMode");
        fogColor = serializedObject.FindProperty("fogColor");
        fogDensity = serializedObject.FindProperty("fogDensity");
        fogStartDistance = serializedObject.FindProperty("fogStartDistance");
        fogEndDistance = serializedObject.FindProperty("fogEndDistance");
        cloudData = serializedObject.FindProperty("cloudData");
        cloudMode = cloudData.FindPropertyRelative("mode");
        cloudMat = cloudData.FindPropertyRelative("cloudMaterial");
        skydomeMesh = cloudData.FindPropertyRelative("skydomeMesh");
        cloudHeight = cloudData.FindPropertyRelative("cloudHeight");
        cloudScale = cloudData.FindPropertyRelative("cloudScale");
        particleCloudData = serializedObject.FindProperty("particleCloudData");
        numClouds = particleCloudData.FindPropertyRelative("numClouds");
        particlesPerCloud = particleCloudData.FindPropertyRelative("particlesPerCloud");
        cloudParticleSize = particleCloudData.FindPropertyRelative("cloudParticleSize");
        cloudScaleMin = particleCloudData.FindPropertyRelative("cloudScaleMin");
        cloudScaleMax = particleCloudData.FindPropertyRelative("cloudScaleMax");
        particleCloudMaterial = particleCloudData.FindPropertyRelative("cloudMaterial");
        particleCloudShadowMaterial = particleCloudData.FindPropertyRelative("cloudShadowMaterial");
        particleCloudColor = particleCloudData.FindPropertyRelative("cloudColor");
        particleCloudLining = particleCloudData.FindPropertyRelative("cloudLining");
        cloudMinSpeed = particleCloudData.FindPropertyRelative("minSpeed");
        cloudMaxSpeed = particleCloudData.FindPropertyRelative("maxSpeed");
        cloudRange = particleCloudData.FindPropertyRelative("cloudRange");
        rainData = serializedObject.FindProperty("rainData");
        rainMaxParticles = rainData.FindPropertyRelative("maxParticles");
        rainEmissionRate = rainData.FindPropertyRelative("emissionRate");
        rainParticleLifetime = rainData.FindPropertyRelative("particleLifetime");
        rainStartSpeed = rainData.FindPropertyRelative("startSpeed");
        rainStartSize = rainData.FindPropertyRelative("startSize");
        rainColor = rainData.FindPropertyRelative("rainColor");
        rainGravityModifier = rainData.FindPropertyRelative("gravityModifier");
        rainEnableCollision = rainData.FindPropertyRelative("enableCollision");
        rainEnableSplashes = rainData.FindPropertyRelative("enableSplashes");
        rainMaterial = rainData.FindPropertyRelative("rainMaterial");
        rainSplashMaterial = rainData.FindPropertyRelative("splashMaterial");
        rainWindForce = rainData.FindPropertyRelative("windForce");
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

            // Toggle for Ridged Multifractal noise (sharp ridges vs smooth hills)
            EditorGUILayout.PropertyField(useRidgedNoise, new GUIContent("Use Ridged Noise",
                "Use Ridged Multifractal instead of standard FBM. Creates sharp ridges like eroded mountains."));

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

            vegMapTable = GUITableLayout.DrawTable(vegMapTable, vegetation);

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

            EditorGUILayout.IntSlider(detailObjectDistance, 0, 10000, new GUIContent("Max Detail Distance"));
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
                terrain.terrain.detailObjectDistance = detailObjectDistance.intValue;
            }
        }

        // ---------------------------
        // Water Section
        // ---------------------------
        showWater = EditorGUILayout.Foldout(showWater, "Water");
        if (showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);

            EditorGUILayout.Slider(waterHeight, 0f, 1f, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO, new GUIContent("Water Prefab"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Water"))
            {
                terrain.AddWater();
            }
            if (GUILayout.Button("Remove Water"))
            {
                terrain.RemoveWater();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------
        // Erosion Section
        // ---------------------------
        showErosion = EditorGUILayout.Foldout(showErosion, "Erosion");
        if (showErosion)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Erosion", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(erosionType, new GUIContent("Erosion Type"));
            EditorGUILayout.Slider(erosionStrength, 0.001f, 1f, new GUIContent("Erosion Strength"));
            EditorGUILayout.Slider(erosionAmount, 0.001f, 1f, new GUIContent("Erosion Amount"));
            EditorGUILayout.IntSlider(droplets, 1, 500, new GUIContent("Droplets"));
            EditorGUILayout.Slider(solubility, 0.001f, 1f, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(springsPerRiver, 1, 20, new GUIContent("Springs Per River"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));
            EditorGUILayout.Slider(windDirection, 0f, 360f, new GUIContent("Wind Direction"));

            if (GUILayout.Button("Erode"))
            {
                terrain.Erode();
            }
        }

        // ---------------------------
        // Fog Section
        // ---------------------------
        showFog = EditorGUILayout.Foldout(showFog, "Fog");
        if (showFog)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Fog Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(enableFog, new GUIContent("Enable Fog"));
            EditorGUILayout.PropertyField(fogMode, new GUIContent("Fog Mode"));
            EditorGUILayout.PropertyField(fogColor, new GUIContent("Fog Color"));

            // Show density for Exponential modes
            if (fogMode.enumValueIndex != (int)FogMode.Linear)
            {
                EditorGUILayout.Slider(fogDensity, 0f, 0.1f, new GUIContent("Fog Density"));
            }

            // Show start/end distance for Linear mode
            if (fogMode.enumValueIndex == (int)FogMode.Linear)
            {
                EditorGUILayout.PropertyField(fogStartDistance, new GUIContent("Start Distance"));
                EditorGUILayout.PropertyField(fogEndDistance, new GUIContent("End Distance"));
            }

            if (GUILayout.Button("Apply Fog"))
            {
                terrain.ApplyFog();
            }
        }

        // ---------------------------
        // Clouds Section
        // ---------------------------
        showClouds = EditorGUILayout.Foldout(showClouds, "Clouds");
        if (showClouds)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Display CloudData properties (cached in OnEnable for performance)
            EditorGUILayout.PropertyField(cloudMode, new GUIContent("Cloud Mode"));

            bool isSkydome = cloudMode.enumValueIndex == (int)CustomTerrain.CloudMode.Skydome;
            if (isSkydome)
            {
                GUILayout.Label("Skydome Settings", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Uses a GeoSphere mesh with animated cloud shader. Avoids UV pinch points at poles.", MessageType.Info);
                EditorGUILayout.PropertyField(skydomeMesh, new GUIContent("Skydome Mesh", "Assign GeoSphere.fbx for best results"));
            }
            else
            {
                GUILayout.Label("Cloud Plane Settings", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Uses a simple plane with animated cloud shader. Very performant (2 triangles).", MessageType.Info);
            }

            EditorGUILayout.PropertyField(cloudMat, new GUIContent("Cloud Material", "Assign Clouds.mat"));
            EditorGUILayout.Slider(cloudHeight, 50f, 500f, new GUIContent("Cloud Height"));

            // Different scale ranges for each mode
            if (isSkydome)
            {
                EditorGUILayout.Slider(cloudScale, 1f, 5f, new GUIContent("Scale", "GeoSphere is ~2000 units, scale 2 = 4000 unit diameter"));
            }
            else
            {
                EditorGUILayout.Slider(cloudScale, 5f, 20f, new GUIContent("Scale Multiplier", "Increase to hide plane edges at horizon"));
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Clouds"))
            {
                terrain.GenerateClouds();
            }
            if (GUILayout.Button("Remove Clouds"))
            {
                terrain.RemoveClouds();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------
        // Particle Clouds Section (volumetric clouds using billboards)
        // ---------------------------
        showParticleClouds = EditorGUILayout.Foldout(showParticleClouds, "Particle Clouds");
        if (showParticleClouds)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Volumetric Cloud System", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates clouds using particle billboards. More expensive than shader clouds.", MessageType.Warning);

            EditorGUILayout.IntSlider(numClouds, 1, 20, new GUIContent("Number of Clouds"));
            EditorGUILayout.IntSlider(particlesPerCloud, 10, 100, new GUIContent("Particles Per Cloud"));
            EditorGUILayout.Slider(cloudParticleSize, 1f, 20f, new GUIContent("Particle Size"));
            EditorGUILayout.PropertyField(cloudScaleMin, new GUIContent("Cloud Scale Min"));
            EditorGUILayout.PropertyField(cloudScaleMax, new GUIContent("Cloud Scale Max"));
            EditorGUILayout.PropertyField(particleCloudMaterial, new GUIContent("Cloud Material"));
            EditorGUILayout.PropertyField(particleCloudShadowMaterial, new GUIContent("Shadow Material"));
            EditorGUILayout.PropertyField(particleCloudColor, new GUIContent("Cloud Color"));
            EditorGUILayout.PropertyField(particleCloudLining, new GUIContent("Lining Color"));
            EditorGUILayout.Slider(cloudMinSpeed, 0.01f, 2f, new GUIContent("Min Speed"));
            EditorGUILayout.Slider(cloudMaxSpeed, 0.1f, 5f, new GUIContent("Max Speed"));
            EditorGUILayout.Slider(cloudRange, 100f, 1000f, new GUIContent("Cloud Range"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Particle Clouds"))
            {
                terrain.GenerateParticleClouds();
            }
            if (GUILayout.Button("Remove Particle Clouds"))
            {
                terrain.RemoveParticleClouds();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------
        // Rain Section
        // ---------------------------
        showRain = EditorGUILayout.Foldout(showRain, "Rain");
        if (showRain)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Rain Particle System", EditorStyles.boldLabel);

            // Display all RainData properties (cached in OnEnable for performance)
            EditorGUILayout.IntSlider(rainMaxParticles, 500, 10000, new GUIContent("Max Particles"));
            EditorGUILayout.Slider(rainEmissionRate, 50f, 2000f, new GUIContent("Emission Rate"));
            EditorGUILayout.Slider(rainParticleLifetime, 1f, 10f, new GUIContent("Particle Lifetime"));
            EditorGUILayout.Slider(rainStartSpeed, 5f, 50f, new GUIContent("Start Speed"));
            EditorGUILayout.PropertyField(rainStartSize, new GUIContent("Start Size (Min/Max)"));
            EditorGUILayout.PropertyField(rainColor, new GUIContent("Rain Color"));
            EditorGUILayout.Slider(rainGravityModifier, 0f, 3f, new GUIContent("Gravity Modifier"));
            EditorGUILayout.PropertyField(rainEnableCollision, new GUIContent("Enable Collision"));
            EditorGUILayout.PropertyField(rainEnableSplashes, new GUIContent("Enable Splashes"));
            EditorGUILayout.PropertyField(rainMaterial, new GUIContent("Rain Material"));
            EditorGUILayout.PropertyField(rainSplashMaterial, new GUIContent("Splash Material"));
            EditorGUILayout.PropertyField(rainWindForce, new GUIContent("Wind Force", "Applies constant force to rain particles (X, Y, Z)"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Rain"))
            {
                terrain.GenerateRain();
            }
            if (GUILayout.Button("Remove Rain"))
            {
                terrain.RemoveRain();
            }
            EditorGUILayout.EndHorizontal();
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
        EditorGUI.indentLevel--;
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Always call this at the end to apply any changes
        serializedObject.ApplyModifiedProperties();
    }
}