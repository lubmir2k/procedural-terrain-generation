# Unity TerrainData API Reference

This document outlines the key TerrainData API methods and their practical use cases for procedural terrain generation.

## Heightmap Operations

| API | Description | Use Cases |
|-----|-------------|-----------|
| `GetHeights()` / `SetHeights()` | Read/write normalized height values [0,1] | Procedural terrain generation (Perlin noise, diamond-square), runtime terrain deformation (craters from explosions, digging mechanics) |
| `GetInterpolatedHeight()` | Sample height at any world position | Spawning objects at correct Y position, AI pathfinding cost calculations, checking if player is above/below certain elevation |
| `heightmapResolution` | Get/set heightmap size (power of 2 + 1) | Adjusting terrain detail for different platforms (lower res for mobile) |

## Texture/Splatmap Control

| API | Description | Use Cases |
|-----|-------------|-----------|
| `GetAlphamaps()` / `SetAlphamaps()` | Read/write texture blend weights per pixel | Auto-texturing by height (snow on peaks, grass in valleys), painting paths/roads procedurally, blending sand near water |
| `terrainLayers` | Assign terrain layer assets (textures, normal maps, tiling) | Swapping texture sets for different biomes, seasonal changes (green grass to brown autumn) |
| `alphamapResolution` | Control splatmap resolution | Performance tuning - lower res for distant terrains in open worlds |

## Vegetation (Trees)

| API | Description | Use Cases |
|-----|-------------|-----------|
| `treeInstances` | Get/set all tree instances (position, scale, rotation, prototype index) | Procedural forest generation, deforestation gameplay (removing trees), saving/loading world state |
| `treePrototypes` | Define available tree prefabs | Biome-specific trees (pine in mountains, palm near coast), LOD swapping tree models |
| `SetTreeInstance()` | Modify individual trees | Tree growth simulation, individual tree destruction without rebuilding entire array |

## Detail Objects (Grass/Rocks)

| API | Description | Use Cases |
|-----|-------------|-----------|
| `GetDetailLayer()` / `SetDetailLayer()` | Read/write detail density maps per layer | Grass that avoids paths/buildings, wildfire spreading (removing grass), farmland crops |
| `detailPrototypes` | Define detail meshes or billboard textures | Seasonal foliage changes, biome-specific ground cover |
| `detailResolution` | Control detail map resolution | Quality scaling - dense grass on PC, sparse on mobile |

## Geometry

| API | Description | Use Cases |
|-----|-------------|-----------|
| `size` | Terrain dimensions (width, height, length) | Matching terrain to world bounds, scaling heightmap imports to desired dimensions |
| `GetSteepness()` | Query slope angle at any point | Placement rules (no trees on cliffs), vehicle physics (slip on steep slopes), AI navigation costs |
| `GetInterpolatedNormal()` | Get surface normal at any point | Aligning objects to surface (rocks, buildings), calculating lighting for custom shaders |

## Holes (Unity 2019.3+)

| API | Description | Use Cases |
|-----|-------------|-----------|
| `GetHoles()` / `SetHoles()` | Create holes in terrain | Cave entrances, mine shafts, tunnel systems, destructible terrain (artillery craters revealing underground) |

## Project Examples

### Height-based Texture Blending (SplatMaps)

```csharp
// From CustomTerrain.cs - applies textures based on slope
float steepness = terrainData.GetSteepness(
    x / (float)(alphaWidth - 1),
    y / (float)(alphaHeight - 1)
);

if (steepness > splatHeights[i].minSlope && steepness < splatHeights[i].maxSlope)
    // Apply rock texture to steep areas
```

### Midpoint Displacement Algorithm

```csharp
// Diamond-square terrain generation
float[,] heightMap = terrainData.GetHeights(0, 0, width, height);
// ... modify heightMap with algorithm ...
terrainData.SetHeights(0, 0, heightMap);
```

### Tree Placement with Raycasting

```csharp
// PlantVegetation() uses height queries for accurate placement
float height = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
if (height > vegetation.minHeight && height < vegetation.maxHeight)
    // Spawn tree at this location
```

## Best Practices

1. **Always clamp heights** - Keep values in [0,1] range after modifications
2. **Double-buffer neighbor operations** - Read from original, write to result array
3. **Cache terrain references** - Avoid `GetComponent<Terrain>()` in loops
4. **Check resolution limits** - Heightmap must be power of 2 + 1 (e.g., 513, 1025)
5. **Batch height changes** - Call `SetHeights()` once rather than per-pixel