using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;

/// <summary>
/// One-click setup for atmospheric effects as described in the course lecture.
/// Sets up fog, camera background, and cloud plane for seamless sky blending.
/// </summary>
public static class AtmosphericSetup
{
    // Default atmospheric settings
    private static readonly Color SkyFogColor = new Color(0.75f, 0.78f, 0.82f, 1f);
    private const float DefaultFogDensity = 0.003f;
    private const float DefaultCloudHeight = 150f;
    private const float DefaultCloudScale = 15f;

    [MenuItem("Tools/Terrain Test/Setup Atmospheric Scene (Lecture Style)")]
    public static void SetupAtmosphericScene()
    {
        // 1. Set up fog in RenderSettings
        SetupFog();

        // 2. Set up camera background to solid color
        SetupCameraBackground();

        // 3. Generate cloud plane if terrain exists
        SetupCloudPlane();

        // 4. Set scene view preferences hint
        Debug.Log("Atmospheric scene setup complete!\n" +
            "- Fog enabled (Exponential Squared, density 0.003)\n" +
            "- Camera background set to solid color\n" +
            "- Cloud plane generated\n\n" +
            "TIP: In Scene view, click the 'Gizmos' dropdown and turn OFF 'Skybox' for best preview.\n" +
            "Or go to Unity > Settings > Colors > Scene Background and set it to match the fog color.");
    }

    [MenuItem("Tools/Terrain Test/Setup Fog Only")]
    public static void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = SkyFogColor;
        RenderSettings.fogDensity = DefaultFogDensity;

        // Also update CustomTerrain if present
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.enableFog = true;
            terrain.fogMode = FogMode.ExponentialSquared;
            terrain.fogColor = SkyFogColor;
            terrain.fogDensity = DefaultFogDensity;
            EditorUtility.SetDirty(terrain);
        }

        Debug.Log($"Fog setup complete: ExponentialSquared, density {DefaultFogDensity}");
    }

    [MenuItem("Tools/Terrain Test/Setup Camera Background")]
    public static void SetupCameraBackground()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("No Main Camera found in scene.");
            return;
        }

        // Set camera to solid color background (not skybox)
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = SkyFogColor;

        EditorUtility.SetDirty(mainCam);
        Debug.Log("Camera background set to solid color matching fog.");
    }

    static void SetupCloudPlane()
    {
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain == null)
        {
            Debug.LogWarning("No CustomTerrain found. Skipping cloud plane generation.");
            return;
        }

        // Check if cloud material is assigned
        if (terrain.cloudData.cloudMaterial == null)
        {
            // Try to find and assign Clouds.mat using LINQ
            var matchingPaths = AssetDatabase.FindAssets("Clouds t:Material")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => System.IO.Path.GetFileName(path) == "Clouds.mat")
                .ToList();

            if (matchingPaths.Count == 1)
            {
                terrain.cloudData.cloudMaterial = AssetDatabase.LoadAssetAtPath<Material>(matchingPaths[0]);
                Debug.Log($"Auto-assigned {matchingPaths[0]} material.");
            }
            else if (matchingPaths.Count > 1)
            {
                Debug.LogWarning($"Multiple 'Clouds.mat' materials found. Please assign one manually to CustomTerrain. Found at: {string.Join(", ", matchingPaths)}");
            }
        }

        if (terrain.cloudData.cloudMaterial == null)
        {
            Debug.LogWarning("Cloud material not found. Please assign Clouds.mat manually.");
            return;
        }

        // Set cloud parameters for good coverage
        terrain.cloudData.cloudHeight = DefaultCloudHeight;
        terrain.cloudData.cloudScale = DefaultCloudScale;

        // Generate the clouds
        terrain.GenerateClouds();
        EditorUtility.SetDirty(terrain);
    }

    [MenuItem("Tools/Terrain Test/Reset to Skybox")]
    public static void ResetToSkybox()
    {
        // Disable fog
        RenderSettings.fog = false;

        // Reset camera to skybox
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.Skybox;
            EditorUtility.SetDirty(mainCam);
        }

        // Remove clouds
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.enableFog = false;
            terrain.RemoveClouds();
            EditorUtility.SetDirty(terrain);
        }

        Debug.Log("Reset to skybox mode. Fog disabled, clouds removed.");
    }
}
