using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    // ---------------------------
    // Terrain Data
    // ---------------------------
    public Terrain terrain;
    public TerrainData terrainData;

    // ---------------------------
    // Random Heights
    // ---------------------------
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);

    // ---------------------------
    // Load Heights from Texture
    // ---------------------------
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool additiveLoadHeights = false;

    // ---------------------------
    // Perlin Noise
    // ---------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;

    void OnEnable()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("CustomTerrain requires a Terrain component on the same GameObject.", this);
            return;
        }

        terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("The Terrain component is missing a TerrainData asset.", this);
        }
    }

    public void RandomTerrain()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        // Get the heightmap from the terrain
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // Set a random height between our min and max values
                heightMap[x, z] = UnityEngine.Random.Range(
                    randomHeightRange.x,
                    randomHeightRange.y);
            }
        }

        // Apply the modified heightmap back to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        // Create a new empty heightmap (all zeros)
        float[,] heightMap = new float[terrainData.heightmapResolution,
            terrainData.heightmapResolution];

        // Apply the empty heightmap to flatten the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }
        if (heightMapImage == null)
        {
            Debug.LogError("HeightMap Image is not assigned.", this);
            return;
        }

        // Create a new heightmap array
        float[,] heightMap = new float[terrainData.heightmapResolution,
            terrainData.heightmapResolution];

        // Pre-fetch all pixels for performance (avoid GetPixel in loop)
        Color[] mapColors = heightMapImage.GetPixels();
        int mapWidth = heightMapImage.width;
        int mapHeight = heightMapImage.height;

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // Get the pixel from the pre-fetched array at the scaled position
                // Clamp to prevent IndexOutOfRangeException
                int pixelX = Mathf.Clamp((int)(x * heightMapScale.x), 0, mapWidth - 1);
                int pixelZ = Mathf.Clamp((int)(z * heightMapScale.z), 0, mapHeight - 1);
                heightMap[x, z] = mapColors[pixelZ * mapWidth + pixelX].grayscale
                    * heightMapScale.y;
            }
        }

        // Apply the heightmap to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }


    public void LoadTextureAdditive()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }
        if (heightMapImage == null)
        {
            Debug.LogError("HeightMap Image is not assigned.", this);
            return;
        }

        // Get EXISTING heights from terrain instead of creating zeros
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        // Pre-fetch all pixels for performance (avoid GetPixel in loop)
        Color[] mapColors = heightMapImage.GetPixels();
        int mapWidth = heightMapImage.width;
        int mapHeight = heightMapImage.height;

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // Get the pixel from the pre-fetched array at the scaled position
                // Clamp to prevent IndexOutOfRangeException
                int pixelX = Mathf.Clamp((int)(x * heightMapScale.x), 0, mapWidth - 1);
                int pixelZ = Mathf.Clamp((int)(z * heightMapScale.z), 0, mapHeight - 1);
                // ADD texture height to existing height with +=
                heightMap[x, z] += mapColors[pixelZ * mapWidth + pixelX].grayscale
                    * heightMapScale.y;
            }
        }

        // Apply the modified heightmap back to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        // Get the heightmap from the terrain
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                // Generate Perlin noise value based on position and scale
                // Offset allows "seeding" - moving along the infinite noise curve
                heightMap[x, y] = Mathf.PerlinNoise(
                    (x + perlinOffsetX) * perlinXScale,
                    (y + perlinOffsetY) * perlinYScale);
            }
        }

        // Apply the heightmap to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

#if UNITY_EDITOR
    void Start()
    {
        // Load the tag manager from project settings
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );

        // Find the tags property
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Add our custom tags
        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        // Apply the changes
        tagManager.ApplyModifiedProperties();

        // Tag this game object as Terrain
        this.gameObject.tag = "Terrain";
    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        // Loop through existing tags to check if it already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag))
            {
                found = true;
                break;
            }
        }

        // If not found, add the new tag
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }
#endif
}