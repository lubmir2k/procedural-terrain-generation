using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
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
    bool showPerlin = false;

    void OnEnable()
    {
        // Link serialized properties to the actual properties on our CustomTerrain
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        additiveLoadHeights = serializedObject.FindProperty("additiveLoadHeights");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
    }

    public override void OnInspectorGUI()
    {
        // Always call this at the start to sync the serialized object
        serializedObject.Update();

        // Get reference to the terrain component
        CustomTerrain terrain = (CustomTerrain)target;

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

            if (GUILayout.Button("Generate Perlin"))
            {
                terrain.Perlin();
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

        // Always call this at the end to apply any changes
        serializedObject.ApplyModifiedProperties();
    }
}