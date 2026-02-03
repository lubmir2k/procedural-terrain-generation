# Fog & Clouds Setup Guide

This guide covers the manual setup required for the Fog, Clouds, and Rain systems implemented in this project.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Fog System](#fog-system)
- [Shader-Based Clouds](#shader-based-clouds)
- [Particle Clouds](#particle-clouds)
- [Rain System](#rain-system)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### 1. Add Decal Render Feature (Required for Cloud Shadows)

Cloud shadows use URP's Decal system which must be enabled in your renderer.

**Steps:**
1. Go to **Edit > Project Settings > Graphics**
2. Click on the **Scriptable Render Pipeline Settings** asset to select it
3. In the Project window, locate the **Universal Renderer Data** asset (typically named `*_Renderer` next to your URP Asset)
4. Select it and in the Inspector, scroll to **Renderer Features**
5. Click **Add Renderer Feature** and select **Decal**

```
Location:
Project Settings > Graphics > URP Asset
    └── Find: [ProjectName]_Renderer (UniversalRendererData)
        └── Add Renderer Feature > Decal
```

### 2. Verify "Sky" Layer Exists

The Sky layer prevents cloud shadows from projecting onto clouds themselves.

**Steps:**
1. Go to **Edit > Project Settings > Tags and Layers**
2. Scroll to the **Layers** section
3. Verify **Sky** exists in User Layers (slots 8-31)
4. If missing, type "Sky" into any empty slot

> **Note:** The `CustomTerrain.Awake()` method auto-creates this layer, but verify it exists if shadows don't work.

---

## Fog System

### Camera Background Setup

Fog only affects terrain geometry, not the skybox. For seamless fog blending, configure your camera:

**Steps:**
1. Select **Main Camera** in the Hierarchy
2. In the Inspector, expand the **Environment** section
3. Change **Background Type** from `Skybox` to `Solid Color`
4. Click the color field and use the **eyedropper tool** to sample your fog color from the scene

### Scene View Background (Editor Only)

The Scene view has its own background color separate from the Game view.

**Steps:**
1. Go to **Unity > Settings** (Mac) or **Edit > Preferences** (Windows)
2. Select **Colors** in the left panel
3. Find **Scene / General / Background**
4. Use the eyedropper to match your fog color

### Using the Fog System

1. Select the **Terrain** GameObject
2. In the CustomTerrain component, expand the **Fog** foldout
3. Configure settings:
   - **Enable Fog**: Toggle fog on/off
   - **Fog Mode**: Linear, Exponential, or ExponentialSquared
   - **Fog Color**: Should match camera background
   - **Fog Density**: Start with 0.003 for subtle fog
   - **Start/End Distance**: Only for Linear mode
4. Click **Apply Fog**

**Quick Menu:** `Tools > Terrain Test > Apply Fog`

---

## Shader-Based Clouds

Shader clouds use a simple plane or skydome with an animated cloud shader. Very performant (2 triangles for plane).

### Assets Required
- `Assets/Clouds.mat` - Cloud material with Shader Graph
- `Assets/Clouds.shadergraph` - Animated cloud shader
- `Assets/Models/GeoSphere.fbx` - Optional geodesic sphere for skydome

### Using Shader Clouds

1. Select the **Terrain** GameObject
2. Expand the **Clouds** foldout
3. Configure:
   - **Cloud Mode**: `Plane` or `Skydome`
   - **Cloud Material**: Assign `Clouds.mat`
   - **Skydome Mesh**: (Skydome mode only) Assign `GeoSphere.fbx`
   - **Cloud Height**: Altitude above terrain (50-500)
   - **Scale**: Size multiplier
4. Click **Generate Clouds**

**Quick Menu:** `Tools > Terrain Test > Generate Clouds`

---

## Particle Clouds

Volumetric clouds using billboard particles. More expensive but provides depth and movement.

### Assets Required

Import the cloud textures from the lecture materials or use the provided assets:

| Asset | Location | Purpose |
|-------|----------|---------|
| Cloud texture | `Assets/Textures/Cloud.png` | White cloud billboard |
| Shadow texture | `Assets/Textures/CloudShadow.png` | Ground shadow projection |
| Cloud material | `Assets/Textures/CloudParticle.mat` | URP Particles/Unlit shader |
| Shadow material | `Assets/Textures/CloudShadowMat.mat` | URP Particles/Unlit shader |

### Material Assignment (Manual Step)

MCP/code cannot reliably assign materials to nested serialized objects. You must manually drag materials:

1. Select the **Terrain** GameObject
2. Expand the **Particle Clouds** foldout
3. Drag and drop:
   - `Assets/Textures/CloudParticle.mat` → **Cloud Material** slot
   - `Assets/Textures/CloudShadowMat.mat` → **Shadow Material** slot

### Configuration Options

| Property | Description | Recommended |
|----------|-------------|-------------|
| Number of Clouds | Total cloud objects | 5-20 |
| Particles Per Cloud | Billboards per cloud | 30-50 |
| Particle Size | Billboard size | 5-15 |
| Cloud Scale Min | Minimum cloud scale (X,Y,Z) | (0.5, 0.5, 0.5) |
| Cloud Scale Max | Maximum cloud scale (X,Y,Z) | (1.5, 1.5, 1.5) |
| Cloud Color | Top/highlight color | White |
| Lining Color | Bottom/shadow color | Gray |
| Min/Max Speed | Movement speed range | 0.1-0.5 |
| Cloud Range | Distance before respawn | 500 |

### Generating Particle Clouds

1. Configure all settings above
2. Click **Generate Particle Clouds**
3. Press **Play** to see clouds moving

**Quick Menu:** `Tools > Terrain Test > Generate Particle Clouds`

---

## Rain System

Particle-based rain with collision splashes and wind force.

### Assets Required

Any particle-compatible material works. The cloud material can be reused:

| Asset | Slot | Purpose |
|-------|------|---------|
| Particle material | Rain Material | Raindrop appearance |
| Particle material | Splash Material | Ground splash effect (optional) |

### Material Assignment (Manual Step)

1. Select the **Terrain** GameObject
2. Expand the **Rain** foldout
3. Drag and drop:
   - `Assets/Textures/CloudParticle.mat` → **Rain Material** slot
   - `Assets/Textures/CloudParticle.mat` → **Splash Material** slot (optional)

### Configuration Options

| Property | Description | Recommended |
|----------|-------------|-------------|
| Max Particles | Particle pool size | 2000-5000 |
| Emission Rate | Particles per second | 300-800 |
| Particle Lifetime | How long drops live | 3-5 seconds |
| Start Speed | Initial fall velocity | 15-30 |
| Start Size | Raindrop size range | (0.01, 0.1) |
| Rain Color | Raindrop tint | Light blue, 50% alpha |
| Gravity Modifier | Fall acceleration | 1.0 |
| Enable Collision | Drops hit terrain | Yes |
| Enable Splashes | Spawn splash on hit | Yes |
| Wind Force | Constant force (X,Y,Z) | (2, 0, 0) for light wind |

### Wind Force

The **Wind Force** property applies constant force to rain particles, simulating wind:
- **X**: East/West wind
- **Y**: Vertical (usually 0)
- **Z**: North/South wind

Example values:
- Light breeze: `(2, 0, 0)`
- Strong wind: `(5, 0, 2)`
- Storm: `(8, 0, 5)`

### Generating Rain

1. Configure all settings above
2. Click **Generate Rain**
3. Press **Play** to see rain falling

**Quick Menu:** `Tools > Terrain Test > Generate Rain`

> **Note:** Rain attaches to the Main Camera and follows the player automatically.

---

## Troubleshooting

### Fog doesn't blend with horizon
- Ensure camera **Background Type** is `Solid Color`
- Match fog color with camera background color
- Match Scene view background in Preferences > Colors

### Cloud shadows not appearing
- Verify **Decal Render Feature** is added to URP Renderer
- Check that **Shadow Material** is assigned
- Verify **Sky** layer exists in Tags and Layers

### Particle clouds invisible
- Assign **Cloud Material** in Inspector (cannot be set via code reliably)
- Check cloud height - may be above camera view
- Press Play - particles may need to initialize

### Rain not generating
- **"Rain Material is not assigned"** error: Manually drag a material to the Rain Material slot
- Check Console for other errors

### Rain splashes not showing
- Assign **Splash Material** in Inspector
- Enable **Enable Splashes** checkbox
- Enable **Enable Collision** checkbox

### Clouds/Rain move with player unnaturally
- This is correct behavior - systems are designed to follow the player
- Simulation Space is set to World so particles stay in place after emission

---

## Quick Setup Checklist

```
□ Add Decal Render Feature to URP Renderer
□ Set Camera Background Type to Solid Color
□ Match Camera background color with fog color
□ Set Scene View background in Preferences > Colors
□ Verify "Sky" layer exists in Tags and Layers
□ Assign Cloud Material to Particle Clouds > Cloud Material
□ Assign Shadow Material to Particle Clouds > Shadow Material
□ Assign Rain Material to Rain > Rain Material
□ (Optional) Assign Splash Material to Rain > Splash Material
□ (Optional) Enable "Always Refresh" in Scene view for preview
```

---

## Related Documentation

- [Design Principles](DesignPrinciples.md) - Terrain design guidelines
- [Erosion Guide](ErosionGuide.md) - Terrain erosion system
- [CLAUDE.md](../../../../CLAUDE.md) - Project overview and architecture
