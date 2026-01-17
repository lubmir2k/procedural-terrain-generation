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

    void OnEnable()
    {
        // Link serialized properties to the actual properties on our CustomTerrain
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        additiveLoadHeights = serializedObject.FindProperty("additiveLoadHeights");
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
                // Call appropriate method based on additive toggle
                if (terrain.additiveLoadHeights)
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