using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

/// <summary>
/// One-click setup for atmospheric effects as described in the course lecture.
/// Sets up fog, camera background, and cloud plane for seamless sky blending.
/// </summary>
public static class AtmosphericSetup
{
    // The fog/sky color that blends everything together (light grey/white)
    private static readonly Color SkyFogColor = new Color(0.75f, 0.78f, 0.82f, 1f);

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
        RenderSettings.fogDensity = 0.003f;

        // Also update CustomTerrain if present
        CustomTerrain terrain = Object.FindFirstObjectByType<CustomTerrain>();
        if (terrain != null)
        {
            terrain.enableFog = true;
            terrain.fogMode = FogMode.ExponentialSquared;
            terrain.fogColor = SkyFogColor;
            terrain.fogDensity = 0.003f;
            EditorUtility.SetDirty(terrain);
        }

        Debug.Log("Fog setup complete: ExponentialSquared, density 0.003");
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
            // Try to find and assign Clouds.mat
            string[] guids = AssetDatabase.FindAssets("Clouds t:Material");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("Clouds.mat"))
                {
                    terrain.cloudData.cloudMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
                    Debug.Log("Auto-assigned Clouds.mat material.");
                    break;
                }
            }
        }

        if (terrain.cloudData.cloudMaterial == null)
        {
            Debug.LogWarning("Cloud material not found. Please assign Clouds.mat manually.");
            return;
        }

        // Set cloud parameters for good coverage
        terrain.cloudData.cloudHeight = 150f;
        terrain.cloudData.cloudScale = 15f;

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
