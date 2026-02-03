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
    // Tag/Layer Management
    // ---------------------------
    public enum TagType { Tag = 0, Layer = 1 }

    [SerializeField]
    int terrainLayer = -1;

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
    public bool useRidgedNoise = false;  // Toggle between FBM and RidgedFBM

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


    // ---------------------------
    // Splatmaps
    // ---------------------------
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public Texture2D textureNormalMap = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float splatOffset = 0.1f;
        public float splatNoiseXScale = 0.01f;
        public float splatNoiseYScale = 0.01f;
        public float splatNoiseScaler = 0.1f;
        public bool remove = false;
    }

    // Vegetation / Trees
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public Color lightColor = Color.white;
        public float minRotation = 0f;
        public float maxRotation = 360f;
        public float minScale = 0.5f;
        public float maxScale = 1.0f;
        public float density = 0.5f;
        public bool remove = false;
    }

    // Detail (grass/rocks)
    [System.Serializable]
    public class Detail
    {
        public GameObject prototype = null;        // For mesh details
        public Texture2D prototypeTexture = null;  // For billboard details
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Color dryColor = Color.white;
        public Color healthyColor = Color.white;
        public Vector2 heightRange = new Vector2(1, 1);  // min/max scale height
        public Vector2 widthRange = new Vector2(1, 1);   // min/max scale width
        public float noiseSpread = 0.5f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
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

    // ---------------------------
    // Splatmaps
    // ---------------------------
    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    // ---------------------------
    // Vegetation / Trees
    // ---------------------------
    public int maxTrees = 5000;
    public int treeSpacing = 5;
    public List<Vegetation> vegetation = new List<Vegetation>()
    {
        new Vegetation()
    };

    // ---------------------------
    // Details (grass/rocks)
    // ---------------------------
    public List<Detail> details = new List<Detail>()
    {
        new Detail()
    };
    public int detailObjectDistance = 5000;
    public int detailSpacing = 5;

    // ---------------------------
    // Water
    // ---------------------------
    public float waterHeight = 0.5f;
    public GameObject waterGO;
    private GameObject _waterInstance;

    // ---------------------------
    // Erosion
    // ---------------------------
    public enum ErosionType { Rain = 0, Thermal = 1, Tidal = 2, River = 3, Wind = 4, Canyon = 5 }
    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public float erosionAmount = 0.01f;
    public int droplets = 10;
    public float solubility = 0.01f;
    public int springsPerRiver = 5;
    public int erosionSmoothAmount = 5;
    public float windDirection = 0f;  // 0-360 degrees

    // ---------------------------
    // Fog
    // ---------------------------
    public bool enableFog = false;
    public FogMode fogMode = FogMode.ExponentialSquared;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;
    public float fogStartDistance = 0f;
    public float fogEndDistance = 300f;

    // ---------------------------
    // Clouds (Skydome)
    // ---------------------------
    public enum CloudMode { Plane = 0, Skydome = 1 }

    [System.Serializable]
    public class CloudData
    {
        public CloudMode mode = CloudMode.Plane;
        public GameObject skydomeMesh;   // GeoSphere mesh for skydome mode
        public Material cloudMaterial;   // Animated cloud shader material
        public float cloudHeight = 200f; // Height above terrain (plane) or dome center
        public float cloudScale = 10f;   // Scale multiplier
    }

    public CloudData cloudData = new CloudData();
    private GameObject _cloudInstance;

    // ---------------------------
    // Rain
    // ---------------------------
    [System.Serializable]
    public class RainData
    {
        public int maxParticles = 3000;
        public float emissionRate = 500f;
        public float particleLifetime = 5f;
        public float startSpeed = 25f;
        public Vector2 startSize = new Vector2(0.01f, 0.1f);
        public Color rainColor = new Color(0.7f, 0.7f, 0.9f, 0.5f);
        public float gravityModifier = 1f;
        public bool enableCollision = true;
        public bool enableSplashes = true;
        public Material rainMaterial;
        public Material splashMaterial;
    }

    public RainData rainData = new RainData();
    private GameObject _rainInstance;

    // Vegetation placement constants
    private const float PositionRandomOffset = 5.0f;
    private const float RaycastHeightOffset = 10f;
    private const float RaycastMaxDistance = 100f;

    // Unity layer constants (user-definable layers start at index 8)
    private const int FirstUserLayerIndex = 8;

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

        // Clear all trees
        terrainData.treeInstances = Array.Empty<TreeInstance>();

        // Clear all detail layers (grass, rocks, etc.)
        terrainData.detailPrototypes = Array.Empty<DetailPrototype>();

        // Clear splatmaps (reset to first layer only)
        int alphamapRes = terrainData.alphamapResolution;
        int numLayers = terrainData.alphamapLayers;
        if (numLayers > 0)
        {
            float[,,] alphaMap = new float[alphamapRes, alphamapRes, numLayers];
            // Set first layer to 1.0, all others remain 0.0
            for (int y = 0; y < alphamapRes; y++)
            {
                for (int x = 0; x < alphamapRes; x++)
                {
                    alphaMap[y, x, 0] = 1f;
                }
            }
            terrainData.SetAlphamaps(0, 0, alphaMap);
        }
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
                // Calculate scaled position with offset
                float xPos = (x + perlinOffsetX) * perlinXScale;
                float zPos = (z + perlinOffsetY) * perlinYScale;

                // Use either standard FBM or RidgedFBM based on toggle
                float noiseValue = useRidgedNoise
                    ? Utils.RidgedFBM(xPos, zPos, perlinOctaves, perlinPersistence)
                    : Utils.FBM(xPos, zPos, perlinOctaves, perlinPersistence);

                heightMap[x, z] += noiseValue * perlinHeightScale;
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


    // ---------------------------
    // Splatmap Methods
    // ---------------------------
    public void SplatMaps()
    {
#if UNITY_EDITOR
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        if (splatHeights == null || splatHeights.Count == 0)
        {
            Debug.LogError("Splat Heights list cannot be empty.", this);
            return;
        }

        // Create terrain layers from our splatHeights list
        TerrainLayer[] newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spIndex = 0;

        // Define path constant for terrain layers
        const string TERRAIN_LAYER_PATH = "Assets/TerrainLayers";

        // Create folder or clean up existing assets to prevent accumulation
        if (!AssetDatabase.IsValidFolder(TERRAIN_LAYER_PATH))
        {
            AssetDatabase.CreateFolder("Assets", "TerrainLayers");
        }
        else
        {
            // Clean up old terrain layers to prevent orphaned assets
            string[] oldLayerGUIDs = AssetDatabase.FindAssets("t:TerrainLayer", new[] { TERRAIN_LAYER_PATH });
            foreach (string guid in oldLayerGUIDs)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        foreach (SplatHeights sh in splatHeights)
        {
            newSplatPrototypes[spIndex] = new TerrainLayer();
            newSplatPrototypes[spIndex].diffuseTexture = sh.texture;
            newSplatPrototypes[spIndex].normalMapTexture = sh.textureNormalMap;
            newSplatPrototypes[spIndex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spIndex].tileSize = sh.tileSize;

            // Create asset with unique path
            string path = AssetDatabase.GenerateUniqueAssetPath(
                TERRAIN_LAYER_PATH + "/TerrainLayer_" + spIndex + ".terrainlayer");
            AssetDatabase.CreateAsset(newSplatPrototypes[spIndex], path);

            spIndex++;
        }

        terrainData.terrainLayers = newSplatPrototypes;

        // Get height data directly (don't use GetHeightMap which may reset)
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        // Create 3D array for splatmap data: [x, y, texture layer]
        float[,,] splatmapData = new float[terrainData.alphamapWidth,
                                           terrainData.alphamapHeight,
                                           terrainData.alphamapLayers];

        // Process each point in the alphamap
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Convert alphamap coordinates to heightmap coordinates once per point
                // (they may have different resolutions)
                int heightMapX = (int)(x * (float)(terrainData.heightmapResolution - 1) / 
                                      (terrainData.alphamapWidth - 1));
                int heightMapY = (int)(y * (float)(terrainData.heightmapResolution - 1) / 
                                      (terrainData.alphamapHeight - 1));

                // Clamp to valid range
                heightMapX = Mathf.Clamp(heightMapX, 0, terrainData.heightmapResolution - 1);
                heightMapY = Mathf.Clamp(heightMapY, 0, terrainData.heightmapResolution - 1);

                float terrainHeight = heightMap[heightMapX, heightMapY];

                // Get steepness (slope angle) once per point using normalized coordinates
                float normX = x * 1.0f / (terrainData.alphamapWidth - 1);
                float normY = y * 1.0f / (terrainData.alphamapHeight - 1);
                float angle = terrainData.GetSteepness(normX, normY);

                // Array to hold splat weights for this position
                float[] splat = new float[terrainData.alphamapLayers];
                bool emptySplat = true;

                for (int i = 0; i < splatHeights.Count; i++)
                {
                    // Add noise for natural-looking edges
                    float noise = Mathf.PerlinNoise(x * splatHeights[i].splatNoiseXScale,
                                                    y * splatHeights[i].splatNoiseYScale)
                                                    * splatHeights[i].splatNoiseScaler;

                    float offset = splatHeights[i].splatOffset + noise;

                    // Calculate height range with offset
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;

                    // Check both height range AND slope range
                    if ((terrainHeight >= thisHeightStart && terrainHeight <= thisHeightStop) &&
                        (angle >= splatHeights[i].minSlope && angle <= splatHeights[i].maxSlope))
                    {
                        // Guard against division by zero when offset is very small
                        if (Mathf.Abs(offset) > float.Epsilon)
                        {
                            // Check if we're in the lower fade zone (near minHeight)
                            if (terrainHeight <= thisHeightStart + offset)
                            {
                                // Fade from 0 to 1 as we move from the lower edge inwards
                                splat[i] = Mathf.InverseLerp(thisHeightStart, thisHeightStart + offset, terrainHeight);
                            }
                            // Check if we're in the upper fade zone (near maxHeight)
                            else if (terrainHeight >= thisHeightStop - offset)
                            {
                                // Fade from 1 to 0 as we approach the upper edge
                                splat[i] = Mathf.InverseLerp(thisHeightStop, thisHeightStop - offset, terrainHeight);
                            }
                            else
                            {
                                // Fully inside the range - full opacity
                                splat[i] = 1;
                            }
                        }
                        else
                        {
                            // Offset is effectively zero - use full opacity
                            splat[i] = 1;
                        }
                        emptySplat = false;
                    }
                }

                // Normalize so all values add up to 1
                NormalizeVector(ref splat);

                // Apply splat values to the splatmap data
                if (emptySplat)
                {
                    // No texture matched - use first texture as fallback
                    splatmapData[x, y, 0] = 1;
                }
                else
                {
                    for (int j = 0; j < splatHeights.Count; j++)
                    {
                        splatmapData[x, y, j] = splat[j];
                    }
                }
            }
        }

        // Apply the splatmap to the terrain
        terrainData.SetAlphamaps(0, 0, splatmapData);

        // Keep selection on the terrain GameObject
        Selection.activeObject = this.gameObject;
#endif
    }


    /// <summary>
    /// Normalizes a vector so all values add up to 1.
    /// Used for splatmap weights where all texture alphas must sum to 1.
    /// </summary>
    void NormalizeVector(ref float[] v)
    {
        float total = 0;

        // Sum all values
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        // If array is empty (all zeros), return early to avoid NaN from division by zero
        if (Mathf.Approximately(total, 0f)) return;

        // Divide each by total to normalize
        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    public void AddSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        // Remove all entries marked for removal (consistent with RemovePerlin pattern)
        splatHeights.RemoveAll(s => s.remove);

        // Ensure at least one entry remains (GUITable requirement)
        if (splatHeights.Count == 0)
        {
            splatHeights.Add(new SplatHeights());
        }
    }


    // ---------------------------
    // Vegetation Methods
    // ---------------------------
    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        vegetation.RemoveAll(v => v.remove);

        if (vegetation.Count == 0)
        {
            vegetation.Add(new Vegetation());
        }
    }

    public void PlantVegetation()
    {
        // First, set up tree prototypes from our vegetation list
        TreePrototype[] newTreePrototypes = new TreePrototype[vegetation.Count];
        int tindex = 0;

        foreach (Vegetation t in vegetation)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.mesh;
            tindex++;
        }

        terrainData.treePrototypes = newTreePrototypes;

        // List to hold all tree instances we create
        List<TreeInstance> allVegetation = new List<TreeInstance>();

        // Get alphamap dimensions for positioning
        int taH = terrainData.alphamapHeight;
        int taW = terrainData.alphamapWidth;

        // Get heightmap dimensions for GetHeight coordinate conversion
        int hmH = terrainData.heightmapResolution;
        int hmW = terrainData.heightmapResolution;

        // Loop through terrain with tree spacing
        for (int z = 0; z < taH; z += treeSpacing)
        {
            for (int x = 0; x < taW; x += treeSpacing)
            {
                // Loop through each tree prototype
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    // Density check - skip placement randomly based on density value
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetation[tp].density)
                    {
                        continue;
                    }

                    // Convert alphamap coordinates to heightmap coordinates for GetHeight
                    int hmX = (int)(((float)x / taW) * hmW);
                    int hmZ = (int)(((float)z / taH) * hmH);

                    // Get height at this position (normalized 0-1)
                    float thisHeight = terrainData.GetHeight(hmX, hmZ) / terrainData.size.y;

                    // Get height constraints from vegetation table
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightEnd = vegetation[tp].maxHeight;

                    // Get steepness at this position (using normalized coordinates)
                    float steepness = terrainData.GetSteepness(
                        (float)x / taW,
                        (float)z / taH
                    );

                    // Check height and slope constraints
                    if (thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd &&
                        steepness >= vegetation[tp].minSlope && steepness <= vegetation[tp].maxSlope)
                    {
                    // Create a new tree instance
                    TreeInstance instance = new TreeInstance();

                    // Set position with random offset to avoid grid alignment
                    instance.position = new Vector3(
                        (x + UnityEngine.Random.Range(-PositionRandomOffset, PositionRandomOffset)) / taW,
                        thisHeight,
                        (z + UnityEngine.Random.Range(-PositionRandomOffset, PositionRandomOffset)) / taH
                    );

                    // Raycast to get accurate height (fixes floating/buried trees)
                    Vector3 treeWorldPos = new Vector3(
                        instance.position.x * terrainData.size.x,
                        instance.position.y * terrainData.size.y,
                        instance.position.z * terrainData.size.z
                    ) + this.transform.position;

                    RaycastHit hit;
                    int layerMask = 1 << terrainLayer;

                    // Raycast down from above, or up from below (for buried trees)
                    if (Physics.Raycast(treeWorldPos + new Vector3(0, RaycastHeightOffset, 0), -Vector3.up, out hit, RaycastMaxDistance, layerMask) ||
                        Physics.Raycast(treeWorldPos - new Vector3(0, RaycastHeightOffset, 0), Vector3.up, out hit, RaycastMaxDistance, layerMask))
                    {
                        float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                        instance.position = new Vector3(instance.position.x, treeHeight, instance.position.z);
                    }

                    // Set rotation for variety
                    instance.rotation = UnityEngine.Random.Range(
                        vegetation[tp].minRotation,
                        vegetation[tp].maxRotation
                    );

                    // Link to the tree prototype
                    instance.prototypeIndex = tp;

                    // Set color with lerp between color1 and color2
                    instance.color = Color.Lerp(
                        vegetation[tp].color1,
                        vegetation[tp].color2,
                        UnityEngine.Random.Range(0.0f, 1.0f)
                    );

                    // Set light map color
                    instance.lightmapColor = vegetation[tp].lightColor;

                    // Set scale for variety
                    float s = UnityEngine.Random.Range(
                        vegetation[tp].minScale,
                        vegetation[tp].maxScale
                    );
                    instance.heightScale = s;
                    instance.widthScale = s;

                    // Add to our list
                    allVegetation.Add(instance);

                    // Check if we've hit max trees
                    if (allVegetation.Count >= maxTrees) goto TREESDONE;
                    } // End of height/slope constraint check
                }
            }
        }

    TREESDONE:
        // Apply all tree instances to the terrain
        terrainData.treeInstances = allVegetation.ToArray();
    }

    // ---------------------------
    // Detail Methods
    // ---------------------------
    public void AddNewDetail()
    {
        details.Add(new Detail());
    }

    public void RemoveDetail()
    {
        details.RemoveAll(d => d.remove);

        if (details.Count == 0)
        {
            details.Add(new Detail());
        }
    }

    public void AddDetails()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        if (detailSpacing < 1)
        {
            Debug.LogError("Detail Spacing must be at least 1 to avoid an infinite loop.", this);
            return;
        }

        // Clear existing detail layers
        terrainData.detailPrototypes = Array.Empty<DetailPrototype>();

        // Create detail prototypes from our details list
        DetailPrototype[] newDetailPrototypes = new DetailPrototype[details.Count];

        for (int i = 0; i < details.Count; i++)
        {
            newDetailPrototypes[i] = new DetailPrototype();

            if (details[i].prototype != null)
            {
                // Mesh-based detail (rocks, etc.)
                newDetailPrototypes[i].prototype = details[i].prototype;
                newDetailPrototypes[i].usePrototypeMesh = true;
                newDetailPrototypes[i].renderMode = DetailRenderMode.Grass; // Fixes URP transparency
            }
            else if (details[i].prototypeTexture != null)
            {
                // Billboard/texture-based detail (grass, ferns)
                newDetailPrototypes[i].prototypeTexture = details[i].prototypeTexture;
                newDetailPrototypes[i].usePrototypeMesh = false;
                newDetailPrototypes[i].renderMode = DetailRenderMode.GrassBillboard;
            }
            else
            {
                Debug.LogWarning($"Detail layer {i} has no mesh or texture assigned. It will not be visible.", this);
            }

            newDetailPrototypes[i].dryColor = details[i].dryColor;
            newDetailPrototypes[i].healthyColor = details[i].healthyColor;
            newDetailPrototypes[i].minHeight = details[i].heightRange.x;
            newDetailPrototypes[i].maxHeight = details[i].heightRange.y;
            newDetailPrototypes[i].minWidth = details[i].widthRange.x;
            newDetailPrototypes[i].maxWidth = details[i].widthRange.y;
            newDetailPrototypes[i].noiseSpread = details[i].noiseSpread;
        }

        terrainData.detailPrototypes = newDetailPrototypes;

        // Get resolution for detail map
        int detailWidth = terrainData.detailWidth;
        int detailHeight = terrainData.detailHeight;
        int heightmapRes = terrainData.heightmapResolution;

        // Determine max value based on scatter mode
        float maxDetailMapValue = (terrainData.detailScatterMode == DetailScatterMode.CoverageMode) ? 255f : 16f;

        // Use System.Random for better performance in tight loops
        var random = new System.Random();

        // Process each detail prototype
        for (int i = 0; i < details.Count; i++)
        {
            // Skip invalid detail layers (no mesh or texture)
            if (details[i].prototype == null && details[i].prototypeTexture == null)
            {
                continue;
            }

            int[,] detailMap = new int[detailHeight, detailWidth];

            float minHeight = details[i].minHeight;
            float maxHeight = details[i].maxHeight;
            float minSlope = details[i].minSlope;
            float maxSlope = details[i].maxSlope;
            float overlap = details[i].overlap;
            float feather = details[i].feather;
            float density = details[i].density;

            for (int y = 0; y < detailHeight; y += detailSpacing)
            {
                for (int x = 0; x < detailWidth; x += detailSpacing)
                {
                    // Density check
                    if (random.NextDouble() > density)
                    {
                        continue;
                    }

                    // Convert detail coords to heightmap coords (critical coordinate flip!)
                    int xHM = (int)((float)x / detailWidth * heightmapRes);
                    int yHM = (int)((float)y / detailHeight * heightmapRes);

                    // Clamp to valid range
                    xHM = Mathf.Clamp(xHM, 0, heightmapRes - 1);
                    yHM = Mathf.Clamp(yHM, 0, heightmapRes - 1);

                    // Get height (normalized 0-1) - note the coordinate flip: GetHeight(y, x)
                    float terrainHeight = terrainData.GetHeight(yHM, xHM) / terrainData.size.y;

                    // Get steepness with normalized coordinates (also flipped)
                    float steepness = terrainData.GetSteepness(
                        (float)yHM / heightmapRes,
                        (float)xHM / heightmapRes
                    );

                    // Calculate noise-adjusted overlap for soft edges
                    // Noise only affects the overlap amount, not the base height thresholds
                    float noise = Mathf.PerlinNoise(x * feather, y * feather) * overlap;
                    float thisHeightStart = minHeight - noise;
                    float thisHeightEnd = maxHeight + noise;

                    // Check height and slope constraints
                    if (terrainHeight >= thisHeightStart && terrainHeight <= thisHeightEnd &&
                        steepness >= minSlope && steepness <= maxSlope)
                    {
                        // Calculate edge falloff for gradual density reduction at boundaries
                        float edgeFalloff = 1f;

                        // Fade in the overlap regions (not inside the core height range)
                        // Fade in from thisHeightStart to minHeight
                        if (terrainHeight < minHeight)
                        {
                            edgeFalloff = Mathf.InverseLerp(thisHeightStart, minHeight, terrainHeight);
                        }
                        // Fade out from maxHeight to thisHeightEnd
                        else if (terrainHeight > maxHeight)
                        {
                            edgeFalloff = Mathf.InverseLerp(thisHeightEnd, maxHeight, terrainHeight);
                        }

                        // Set detail value scaled by edge falloff (uses y, x ordering - critical!)
                        int detailValue = (int)(random.Next(1, (int)maxDetailMapValue + 1) * edgeFalloff);
                        if (detailValue > 0)
                        {
                            detailMap[y, x] = detailValue;
                        }
                    }
                }
            }

            // Apply this detail layer
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
    }

    // ---------------------------
    // Water Methods
    // ---------------------------
    public void AddWater()
    {
        if (_waterInstance == null)
        {
            if (waterGO == null)
            {
                Debug.LogWarning("Water prefab is not assigned.", this);
                return;
            }
            _waterInstance = Instantiate(waterGO, transform.position, transform.rotation);
            _waterInstance.name = "Water";
        }
        _waterInstance.transform.position = transform.position +
            new Vector3(terrainData.size.x / 2, waterHeight * terrainData.size.y, terrainData.size.z / 2);
        _waterInstance.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
    }

    public void RemoveWater()
    {
        if (_waterInstance != null)
        {
            DestroyImmediate(_waterInstance);
            _waterInstance = null;
        }
    }

    // ---------------------------
    // Erosion Methods
    // ---------------------------
    public void Erode()
    {
        switch (erosionType)
        {
            case ErosionType.Rain:
                Rain();
                break;
            case ErosionType.Thermal:
                Thermal();
                break;
            case ErosionType.Tidal:
                Tidal();
                break;
            case ErosionType.River:
                River();
                break;
            case ErosionType.Wind:
                Wind();
                break;
            case ErosionType.Canyon:
                DigCanyon();
                break;
        }

        // Apply smoothing after erosion
        int savedSmooth = smoothAmount;
        smoothAmount = erosionSmoothAmount;
        Smooth();
        smoothAmount = savedSmooth;
    }

    /// <summary>
    /// Rain erosion - randomly picks points and subtracts height (divots/holes).
    /// </summary>
    void Rain()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        for (int i = 0; i < droplets; i++)
        {
            int x = UnityEngine.Random.Range(0, terrainData.heightmapResolution);
            int y = UnityEngine.Random.Range(0, terrainData.heightmapResolution);
            heightMap[x, y] = Mathf.Max(0f, heightMap[x, y] - erosionStrength);
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Thermal erosion - simulates landslides by moving material from high to low neighbors.
    /// </summary>
    void Thermal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Double-buffer: read from heightMap, write changes to result
        float[,] result = new float[width, height];
        System.Array.Copy(heightMap, result, heightMap.Length);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                List<Vector2> neighbours = Utils.GenerateNeighbours(pos, width, height);

                foreach (Vector2 n in neighbours)
                {
                    int nx = (int)n.x;
                    int ny = (int)n.y;

                    // If current is higher than neighbor by more than erosionStrength (read from original)
                    if (heightMap[x, y] > heightMap[nx, ny] + erosionStrength)
                    {
                        float currentHeight = heightMap[x, y];
                        float transfer = currentHeight * erosionAmount;
                        result[x, y] = Mathf.Max(0f, result[x, y] - transfer);
                        result[nx, ny] = Mathf.Min(1f, result[nx, ny] + transfer);
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, result);
    }

    /// <summary>
    /// Tidal erosion - erodes shorelines at the water level boundary.
    /// </summary>
    void Tidal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Double-buffer: read from heightMap, write changes to result
        float[,] result = new float[width, height];
        System.Array.Copy(heightMap, result, heightMap.Length);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                List<Vector2> neighbours = Utils.GenerateNeighbours(pos, width, height);

                foreach (Vector2 n in neighbours)
                {
                    int nx = (int)n.x;
                    int ny = (int)n.y;

                    // If current is below water AND neighbor is above water (read from original)
                    if (heightMap[x, y] < waterHeight && heightMap[nx, ny] > waterHeight)
                    {
                        // Create beach effect - gradually pull both toward water level using Lerp
                        result[nx, ny] = Mathf.Lerp(result[nx, ny], waterHeight, erosionStrength);
                        result[x, y] = Mathf.Lerp(result[x, y], waterHeight, erosionStrength);
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, result);
    }

    /// <summary>
    /// River erosion - simulates rivers flowing from random springs, carving channels.
    /// </summary>
    void River()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        float[,] erosionMap = new float[terrainData.heightmapResolution,
                                        terrainData.heightmapResolution];

        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        for (int i = 0; i < droplets; i++)
        {
            // Random starting position (spring)
            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height);

            // Run multiple river simulations from this spring
            for (int j = 0; j < springsPerRiver; j++)
            {
                RunRiver(x, y, heightMap, erosionMap, width, height);
            }
        }

        // Apply erosion map to heightmap
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightMap[x, y] = Mathf.Max(0f, heightMap[x, y] - erosionMap[x, y]);
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Runs a single river simulation from a starting point, following the lowest neighbor.
    /// </summary>
    void RunRiver(int startX, int startY, float[,] heightMap, float[,] erosionMap, int width, int height)
    {
        int x = startX;
        int y = startY;
        int iterations = 0;
        int maxIterations = width * height; // Prevent infinite loops

        while (iterations < maxIterations)
        {
            iterations++;
            erosionMap[x, y] += solubility;

            Vector2 pos = new Vector2(x, y);
            List<Vector2> neighbours = Utils.GenerateNeighbours(pos, width, height);

            // Find lowest neighbor
            float minHeight = heightMap[x, y];
            int nextX = x;
            int nextY = y;
            bool foundLower = false;

            Utils.Shuffle(neighbours); // Randomize to avoid bias

            foreach (Vector2 n in neighbours)
            {
                int nx = (int)n.x;
                int ny = (int)n.y;
                if (heightMap[nx, ny] < minHeight)
                {
                    minHeight = heightMap[nx, ny];
                    nextX = nx;
                    nextY = ny;
                    foundLower = true;
                }
            }

            // Stop if no lower neighbor found (reached a basin)
            if (!foundLower)
                break;

            x = nextX;
            y = nextY;
        }
    }

    /// <summary>
    /// Wind erosion - creates ripple patterns based on wind direction using Perlin noise.
    /// </summary>
    void Wind()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Double-buffer: read from heightMap, write changes to result
        float[,] result = new float[width, height];
        System.Array.Copy(heightMap, result, heightMap.Length);

        // Convert wind direction to radians
        float theta = windDirection * Mathf.Deg2Rad;
        float sinTheta = Mathf.Sin(theta);
        float cosTheta = Mathf.Cos(theta);

        // Technical parameters (hardcoded as per plan)
        int stepSize = 10;
        int depositOffset = 5;
        float digAmount = 0.001f;

        // Extended loop bounds for rotation coverage
        for (int y = -(height - 1); y < height; y += stepSize)
        {
            for (int x = 0; x < width; x++)
            {
                // Generate noise for wave pattern
                float noise = Mathf.PerlinNoise((float)x / width, (float)(y + height) / (height * 2)) * erosionStrength;

                // Calculate dig position with rotation
                int digX = (int)(x * cosTheta - y * sinTheta);
                int digY = (int)(y * cosTheta + x * sinTheta);

                // Calculate deposit position (offset from dig)
                int depositX = (int)((x + depositOffset) * cosTheta - y * sinTheta);
                int depositY = (int)(y * cosTheta + (x + depositOffset) * sinTheta);

                // Bounds checking for dig position
                if (digX >= 0 && digX < width && digY >= 0 && digY < height)
                {
                    // Bounds checking for deposit position
                    if (depositX >= 0 && depositX < width && depositY >= 0 && depositY < height)
                    {
                        float amount = (digAmount + noise) * erosionAmount;
                        float actualAmount = Mathf.Min(amount, heightMap[digX, digY]);
                        result[digX, digY] -= actualAmount;
                        result[depositX, depositY] = Mathf.Min(1f, result[depositX, depositY] + actualAmount);
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, result);
    }

    /// <summary>
    /// Canyon erosion - carves meandering canyons across the terrain using iterative crawling.
    /// Uses explicit Stack instead of recursion to prevent StackOverflowException on large terrains.
    /// </summary>
    void DigCanyon()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Start at random edge
        int edge = UnityEngine.Random.Range(0, 4);
        int startX, startY;

        switch (edge)
        {
            case 0: // Top
                startX = UnityEngine.Random.Range(0, width);
                startY = 0;
                break;
            case 1: // Bottom
                startX = UnityEngine.Random.Range(0, width);
                startY = height - 1;
                break;
            case 2: // Left
                startX = 0;
                startY = UnityEngine.Random.Range(0, height);
                break;
            default: // Right
                startX = width - 1;
                startY = UnityEngine.Random.Range(0, height);
                break;
        }

        // Iterative canyon carving using explicit stack (prevents StackOverflowException)
        var stack = new Stack<(int x, int y, float depth, int iteration)>();
        stack.Push((startX, startY, erosionStrength, 0));
        int maxIterations = width + height;

        while (stack.Count > 0)
        {
            var (x, y, currentDepth, iteration) = stack.Pop();

            // Exit conditions
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue; // Out of bounds

            if (currentDepth <= 0)
                continue; // Reached max depth

            if (iteration > maxIterations)
                continue; // Prevent runaway

            // Dig at current position
            heightMap[x, y] = Mathf.Max(0f, heightMap[x, y] - currentDepth);

            // Get neighbors and shuffle for random direction
            Vector2 pos = new Vector2(x, y);
            List<Vector2> neighbours = Utils.GenerateNeighbours(pos, width, height);
            Utils.Shuffle(neighbours);

            // Pick random neighbor to continue canyon
            if (neighbours.Count > 0)
            {
                Vector2 next = neighbours[0];
                stack.Push(((int)next.x, (int)next.y, currentDepth - erosionAmount, iteration + 1));
            }

            // Occasionally branch (V-shaped canyon)
            if (neighbours.Count > 1 && UnityEngine.Random.value < 0.1f)
            {
                Vector2 branch = neighbours[1];
                stack.Push(((int)branch.x, (int)branch.y, currentDepth * 0.5f, iteration + 1));
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    // ---------------------------
    // Helper Methods
    // ---------------------------

    /// <summary>
    /// Safely destroys an object, using Destroy at runtime and DestroyImmediate in editor.
    /// Required for [ExecuteInEditMode] scripts that may run in both contexts.
    /// </summary>
    void SafeDestroy(UnityEngine.Object obj)
    {
        if (obj == null) return;

        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    // ---------------------------
    // Fog Methods
    // ---------------------------
    public void ApplyFog()
    {
        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }

    // ---------------------------
    // Cloud Methods (Plane or Skydome)
    // ---------------------------
    public void GenerateClouds()
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not assigned.", this);
            return;
        }

        // Remove existing clouds first
        RemoveClouds();

        if (cloudData.cloudMaterial == null)
        {
            Debug.LogWarning("Cloud material is not assigned.", this);
            return;
        }

        if (cloudData.mode == CloudMode.Skydome)
        {
            GenerateSkydome();
        }
        else
        {
            GeneratePlane();
        }
    }

    void GeneratePlane()
    {
        // Create a simple plane (2 triangles - very performant)
        _cloudInstance = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _cloudInstance.name = "CloudPlane";

        // Remove collider (not needed for visual-only clouds)
        Collider col = _cloudInstance.GetComponent<Collider>();
        SafeDestroy(col);

        // Position centered over terrain at specified height
        Vector3 terrainCenter = transform.position + new Vector3(
            terrainData.size.x / 2f,
            cloudData.cloudHeight,
            terrainData.size.z / 2f
        );
        _cloudInstance.transform.position = terrainCenter;

        // Scale to cover terrain (plane is 10x10 by default, so divide by 10)
        float scaleX = (terrainData.size.x / 10f) * cloudData.cloudScale;
        float scaleZ = (terrainData.size.z / 10f) * cloudData.cloudScale;
        _cloudInstance.transform.localScale = new Vector3(scaleX, 1f, scaleZ);

        // Apply cloud material
        ApplyCloudMaterial(_cloudInstance);
    }

    void GenerateSkydome()
    {
        if (cloudData.skydomeMesh == null)
        {
            Debug.LogWarning("Skydome mesh (GeoSphere) is not assigned. Using default sphere.", this);
            _cloudInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _cloudInstance.name = "Skydome";
        }
        else
        {
            _cloudInstance = Instantiate(cloudData.skydomeMesh);
            _cloudInstance.name = "Skydome";
        }

        // Remove collider
        Collider col = _cloudInstance.GetComponent<Collider>();
        SafeDestroy(col);

        // Position centered over terrain
        Vector3 terrainCenter = transform.position + new Vector3(
            terrainData.size.x / 2f,
            cloudData.cloudHeight,
            terrainData.size.z / 2f
        );
        _cloudInstance.transform.position = terrainCenter;

        // Scale the skydome - flip Y to invert normals so inside is visible
        // GeoSphere mesh is ~2000 units diameter, so cloudScale 1-5 gives 2000-10000 units
        float scale = cloudData.cloudScale;
        _cloudInstance.transform.localScale = new Vector3(scale, -scale, scale);

        // Apply cloud material
        ApplyCloudMaterial(_cloudInstance);
    }

    void ApplyCloudMaterial(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.sharedMaterial = cloudData.cloudMaterial;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }
    }

    public void RemoveClouds()
    {
        SafeDestroy(_cloudInstance);
        _cloudInstance = null;
    }

    // ---------------------------
    // Rain Methods
    // ---------------------------
    public void GenerateRain()
    {
        // Remove existing rain first
        RemoveRain();

        // Find the main camera to attach rain to
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("No Main Camera found. Rain particle system will be created at terrain position.", this);
        }

        // Create rain particle system
        _rainInstance = new GameObject("RainParticleSystem");

        if (mainCam != null)
        {
            _rainInstance.transform.parent = mainCam.transform;
            _rainInstance.transform.localPosition = new Vector3(0, 20f, 0);
        }
        else
        {
            _rainInstance.transform.position = transform.position + Vector3.up * 100f;
        }

        // Add particle system component
        ParticleSystem ps = _rainInstance.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.maxParticles = rainData.maxParticles;
        main.startLifetime = rainData.particleLifetime;
        main.startSpeed = rainData.startSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(rainData.startSize.x, rainData.startSize.y);
        main.startColor = rainData.rainColor;
        main.gravityModifier = rainData.gravityModifier;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = true;

        // Emission module
        var emission = ps.emission;
        emission.rateOverTime = rainData.emissionRate;

        // Shape module - box shape above the camera
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(50f, 1f, 50f);

        // Renderer module - stretched billboard for rain streaks
        var renderer = _rainInstance.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.1f;
        renderer.lengthScale = 3f;

        if (rainData.rainMaterial != null)
        {
            renderer.material = rainData.rainMaterial;
        }
        else
        {
            Debug.LogWarning("Rain Material is not assigned. Aborting rain generation.", this);
            RemoveRain();
            return;
        }

        // Collision module
        if (rainData.enableCollision)
        {
            var collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.bounce = 0f;
            collision.lifetimeLoss = 1f;
            collision.sendCollisionMessages = rainData.enableSplashes;
        }

        // Sub-emitter for splashes
        if (rainData.enableSplashes && rainData.enableCollision)
        {
            // Create splash sub-emitter
            GameObject splashGO = new GameObject("SplashSubEmitter");
            splashGO.transform.parent = _rainInstance.transform;
            splashGO.transform.localPosition = Vector3.zero;

            ParticleSystem splashPS = splashGO.AddComponent<ParticleSystem>();

            // Splash main module
            var splashMain = splashPS.main;
            splashMain.maxParticles = 1000;
            splashMain.startLifetime = 0.2f;
            splashMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            splashMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            splashMain.startColor = new Color(0.8f, 0.8f, 1f, 0.6f);
            splashMain.gravityModifier = 0.5f;
            splashMain.simulationSpace = ParticleSystemSimulationSpace.World;
            splashMain.playOnAwake = false;
            splashMain.loop = false;

            // Splash emission - controlled by sub-emitter
            var splashEmission = splashPS.emission;
            splashEmission.rateOverTime = 0;

            // Splash shape - hemisphere for radial splash
            var splashShape = splashPS.shape;
            splashShape.shapeType = ParticleSystemShapeType.Hemisphere;
            splashShape.radius = 0.1f;

            // Splash renderer
            var splashRenderer = splashGO.GetComponent<ParticleSystemRenderer>();
            splashRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            if (rainData.splashMaterial != null)
            {
                splashRenderer.material = rainData.splashMaterial;
            }
            else
            {
                Debug.LogWarning("Splash Material is not assigned. Splashes may not render correctly.", this);
            }

            // Add as sub-emitter
            var subEmitters = ps.subEmitters;
            subEmitters.enabled = true;
            subEmitters.AddSubEmitter(splashPS, ParticleSystemSubEmitterType.Collision,
                ParticleSystemSubEmitterProperties.InheritNothing);
        }

        // Start playing
        ps.Play();
    }

    public void RemoveRain()
    {
        SafeDestroy(_rainInstance);
        _rainInstance = null;
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
        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);

        // Apply the changes
        tagManager.ApplyModifiedProperties();

        // Find the layers property and add terrain layer
        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        // Set tag and layer for this game object
        this.gameObject.tag = "Terrain";
        if (terrainLayer != -1)
        {
            this.gameObject.layer = terrainLayer;
        }
        else
        {
            Debug.LogWarning("Could not assign 'Terrain' layer. Make sure there is an empty user layer in Project Settings > Tags and Layers.", this);
        }
    }

    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        // Loop through existing tags/layers to check if it already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag))
            {
                return i; // Return the index if found
            }
        }

        // Add new tag (tags can be inserted at index 0)
        if (tType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
            return 0; // Return index where tag was inserted
        }

        // Add new layer (layers have fixed slots, find empty one)
        if (tType == TagType.Layer)
        {
            for (int j = FirstUserLayerIndex; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                // Add layer in next empty slot
                if (string.IsNullOrEmpty(newLayer.stringValue))
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }

        return -1; // No empty layer slot found
    }
#endif
}