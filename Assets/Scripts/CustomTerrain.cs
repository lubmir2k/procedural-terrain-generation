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
    // Reset Terrain
    // ---------------------------
    public bool resetTerrain = true;

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
    public int perlinOctaves = 3;
    public float perlinPersistence = 0.5f;
    public float perlinHeightScale = 0.09f;

    // ---------------------------
    // Multiple Perlin Noise
    // ---------------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistence = 0.5f;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };


    // ---------------------------
    // Voronoi Tessellation
    // ---------------------------
    public float voronoiFalloff = 0.5f;
    public float voronoiDropoff = 2.0f;
    public float voronoiMinHeight = 0.1f;
    public float voronoiMaxHeight = 0.5f;
    public int voronoiPeaks = 5;
    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, SinPow = 3, Perlin = 4 }
    public VoronoiType voronoiType = VoronoiType.Linear;

    // ---------------------------
    // Midpoint Displacement
    // ---------------------------
    public float MPDheightMin = -10f;
    public float MPDheightMax = 10f;
    public float MPDheightDampenerPower = 2.0f;
    public float MPDroughness = 2.0f;

    // ---------------------------
    // Smooth
    // ---------------------------
    public int smoothAmount = 1;

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

    /// <summary>
    /// Gets or creates a height map based on resetTerrain flag
    /// </summary>
    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            // Return existing heights to add onto
            return terrainData.GetHeights(0, 0,
                terrainData.heightmapResolution,
                terrainData.heightmapResolution);
        }
        else
        {
            // Return a new zeroed height map
            return new float[terrainData.heightmapResolution,
                terrainData.heightmapResolution];
        }
    }

    public void RandomTerrain()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        float[,] heightMap = GetHeightMap();

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // Add random height between our min and max values
                heightMap[x, z] += UnityEngine.Random.Range(
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

        float[,] heightMap = GetHeightMap();

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
                heightMap[x, z] += mapColors[pixelZ * mapWidth + pixelX].grayscale
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

        float[,] heightMap = GetHeightMap();

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // Use Fractal Brownian Motion for more natural terrain
                // Offset is added BEFORE scaling to avoid cubic artifacts
                heightMap[x, z] += Utils.FBM(
                    (x + perlinOffsetX) * perlinXScale,
                    (z + perlinOffsetY) * perlinYScale,
                    perlinOctaves,
                    perlinPersistence) * perlinHeightScale;
            }
        }

        // Apply the heightmap to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        float[,] heightMap = GetHeightMap();

        // Loop through every point in the heightmap
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                // Apply each Perlin parameter set
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, z] += Utils.FBM(
                        (x + p.mPerlinOffsetX) * p.mPerlinXScale,
                        (z + p.mPerlinOffsetY) * p.mPerlinYScale,
                        p.mPerlinOctaves,
                        p.mPerlinPersistence) * p.mPerlinHeightScale;
                }
            }
        }

        // Apply the heightmap to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RidgeNoise()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        // Note: This method intentionally uses GetHeights() instead of GetHeightMap()
        // to always transform the existing terrain, ignoring the resetTerrain flag.
        // RidgeNoise is a post-process effect meant to be applied after generating
        // terrain with other methods (Single Perlin, Multiple Perlin, etc.).
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        // Apply ridge noise transformation to existing terrain
        // Formula: newHeight = 1 - |oldHeight * 2 - 1|
        // This transforms values in the [0, 1] range to create sharp ridges
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] = 1 - Mathf.Abs(heightMap[x, z] * 2f - 1f);
            }
        }

        // Apply the modified heightmap back to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }


    /// <summary>
    /// Generates terrain peaks using a Voronoi-inspired algorithm.
    /// Note: This is a "multiple peak influence" algorithm where each peak's height
    /// contribution is calculated based on distance falloff. It creates overlapping
    /// radial gradients rather than the sharp cell boundaries of true Voronoi tessellation.
    /// The name follows the course material convention.
    /// </summary>
    public void Voronoi()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        float[,] heightMap = GetHeightMap();
        int resolution = terrainData.heightmapResolution;

        // Calculate max distance for normalization once (diagonal of terrain)
        float maxDistance = resolution * Mathf.Sqrt(2f);

        // Generate random peak positions and heights
        for (int p = 0; p < voronoiPeaks; p++)
        {
            // Random position within terrain bounds
            int peakX = UnityEngine.Random.Range(0, resolution);
            int peakZ = UnityEngine.Random.Range(0, resolution);
            float peakHeight = UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight);

            // Set the peak height
            if (heightMap[peakX, peakZ] < peakHeight)
            {
                heightMap[peakX, peakZ] = peakHeight;
            }

            // Raise terrain around the peak based on distance
            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    // Skip the peak itself
                    if (x == peakX && z == peakZ) continue;

                    // Calculate distance manually to avoid Vector2 allocations in tight loop
                    float dx = x - peakX;
                    float dz = z - peakZ;
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);

                    // Normalize distance to 0-1 range
                    float normalizedDistance = distance / maxDistance;

                    // Calculate height contribution based on selected Voronoi type
                    float heightContribution;

                    switch (voronoiType)
                    {
                        case VoronoiType.Combined:
                            // Linear falloff + Power curve
                            heightContribution = peakHeight
                                - (normalizedDistance * voronoiFalloff)
                                - Mathf.Pow(normalizedDistance, voronoiDropoff);
                            break;

                        case VoronoiType.Power:
                            // Power curve with falloff multiplier
                            heightContribution = peakHeight
                                - Mathf.Pow(normalizedDistance, voronoiDropoff) * voronoiFalloff;
                            break;

                        case VoronoiType.SinPow:
                            // Sin + Power combination for meringue-like peaks
                            // Guard against division by zero
                            float sinComponent = !Mathf.Approximately(voronoiDropoff, 0f)
                                ? (Mathf.Sin(normalizedDistance * 2 * Mathf.PI) / voronoiDropoff)
                                : 0f;
                            heightContribution = peakHeight
                                - Mathf.Pow(normalizedDistance * 3, voronoiFalloff)
                                - sinComponent;
                            break;

                        case VoronoiType.Perlin:
                            // Linear falloff combined with Perlin noise for natural-looking slopes
                            heightContribution = (peakHeight - (normalizedDistance * voronoiFalloff))
                                + Utils.FBM(
                                    (x + perlinOffsetX) * perlinXScale,
                                    (z + perlinOffsetY) * perlinYScale,
                                    perlinOctaves,
                                    perlinPersistence) * perlinHeightScale;
                            break;

                        default: // Linear
                            // Simple linear falloff
                            heightContribution = peakHeight - (normalizedDistance * voronoiFalloff);
                            break;
                    }

                    // Only raise terrain, never lower it (for this peak)
                    if (heightContribution > heightMap[x, z])
                    {
                        heightMap[x, z] = heightContribution;
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates terrain using the Midpoint Displacement (Diamond-Square) algorithm.
    /// Creates fractal-like terrain with natural ridges and valleys.
    /// </summary>
    public void MidpointDisplacement()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution - 1;
        int squareSize = width;

        // Use local copies to avoid modifying inspector values during iteration
        float heightMin = MPDheightMin;
        float heightMax = MPDheightMax;
        float heightDampener = (float)Mathf.Pow(MPDheightDampenerPower, -1 * MPDroughness);

        // Coordinate variables for the algorithm
        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        while (squareSize > 0)
        {
            // Diamond Step: Calculate center of each square
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            // Square Step: Calculate midpoint of each edge
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    // Calculate neighbor midpoints (may be outside current square)
                    pmidXR = midX + squareSize;
                    pmidXL = midX - squareSize;
                    pmidYU = midY + squareSize;
                    pmidYD = midY - squareSize;

                    // Skip edge squares to avoid index out of bounds
                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1)
                        continue;

                    // Bottom edge midpoint
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, y] +
                                                  heightMap[midX, pmidYD] +
                                                  heightMap[cornerX, y]) / 4.0f +
                                                  UnityEngine.Random.Range(heightMin, heightMax));

                    // Top edge midpoint
                    heightMap[midX, cornerY] = (float)((heightMap[midX, midY] +
                                                        heightMap[x, cornerY] +
                                                        heightMap[midX, pmidYU] +
                                                        heightMap[cornerX, cornerY]) / 4.0f +
                                                        UnityEngine.Random.Range(heightMin, heightMax));

                    // Left edge midpoint
                    heightMap[x, midY] = (float)((heightMap[x, y] +
                                                  heightMap[pmidXL, midY] +
                                                  heightMap[x, cornerY] +
                                                  heightMap[midX, midY]) / 4.0f +
                                                  UnityEngine.Random.Range(heightMin, heightMax));

                    // Right edge midpoint
                    heightMap[cornerX, midY] = (float)((heightMap[midX, y] +
                                                        heightMap[midX, midY] +
                                                        heightMap[cornerX, y] +
                                                        heightMap[pmidXR, midY]) / 4.0f +
                                                        UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            // Halve the square size and dampen the height range
            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Smooths the terrain by averaging each height value with its neighbors.
    /// Runs the smoothing pass multiple times based on smoothAmount.
    /// </summary>
    public void Smooth()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Use double-buffer to avoid directional smoothing artifacts
        float[,] readMap = terrainData.GetHeights(0, 0, width, height);
        float[,] writeMap = new float[width, height];

        for (int s = 0; s < smoothAmount; s++)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", (float)s / smoothAmount);
#endif
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float avgHeight = 0f;
                    int neighbourCount = 0;

                    // Inline neighbor sampling for performance
                    for (int ny = -1; ny <= 1; ny++)
                    {
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            int sampleX = x + nx;
                            int sampleY = y + ny;

                            if (sampleX >= 0 && sampleX < width && sampleY >= 0 && sampleY < height)
                            {
                                avgHeight += readMap[sampleX, sampleY];
                                neighbourCount++;
                            }
                        }
                    }
                    writeMap[x, y] = avgHeight / neighbourCount;
                }
            }

            // Swap buffers for next pass
            var temp = readMap;
            readMap = writeMap;
            writeMap = temp;
        }

        terrainData.SetHeights(0, 0, readMap);
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        // Remove all parameters marked for removal
        perlinParameters.RemoveAll(p => p.remove);

        // Ensure at least one parameter remains (GUITable requirement)
        if (perlinParameters.Count == 0)
        {
            perlinParameters.Add(new PerlinParameters());
        }
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