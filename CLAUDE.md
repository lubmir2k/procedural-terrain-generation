# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 (6000.3.2f1) procedural terrain generation project with URP rendering pipeline. Cross-platform support for PC, Mobile (Android/iOS), and VR (OpenXR/Oculus).

## Unity Commands

Open project in Unity Hub or via command line:
```bash
# macOS
/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity -projectPath "/Users/lubom/repos/Procedural Terrain Generation"
```

Run tests via Unity Test Framework (Window > General > Test Runner, or use `com.unity.test-framework`).

Quick testing via menu: `Tools > Terrain Test > [Reset|Generate Voronoi|Generate MPD|Smooth|Apply Details]`

## Architecture

### Terrain Generation System (`Assets/Scripts/`)

The core procedural terrain system with custom editor GUI:

**CustomTerrain.cs** - Main MonoBehaviour with `[ExecuteInEditMode]`. Attached to Unity Terrain GameObjects. Generation algorithms:
- `RandomTerrain()` - Random height values in range
- `LoadTexture()` / `LoadTextureAdditive()` - Import heightmaps from textures
- `Perlin()` - Single-layer Perlin noise with FBM
- `MultiplePerlinTerrain()` - Stackable Perlin layers via `PerlinParameters` list
- `RidgeNoise()` - Post-process transform: `1 - |h * 2 - 1|` for sharp ridges
- `Voronoi()` - Peak-based terrain with 5 falloff types (Linear, Power, Combined, SinPow, Perlin)
- `MidpointDisplacement()` - Diamond-square algorithm for fractal terrain
- `Smooth()` - Double-buffered neighbor averaging
- `SplatMaps()` - Height/slope-based texture blending with noise edges
- `PlantVegetation()` - Tree placement by height/slope constraints with raycasting
- `AddDetails()` - Grass billboards and mesh details (rocks) by height/slope constraints

**Utils.cs** - Static helper with `FBM()` (Fractal Brownian Motion) for multi-octave Perlin noise and `Map()` for range remapping.

**CustomTerrainEditor.cs** (`Assets/Editor/`) - Custom inspector using GUITable for parameter lists. Foldable sections for each algorithm.

**TerrainEditorUtility.cs** / **TerrainTestMenu.cs** (`Assets/Editor/`) - Menu items under `Terrain/` and `Tools/Terrain Test/` for quick operations.

### Data Classes (in CustomTerrain.cs)

- `PerlinParameters` - Per-layer noise settings (scale, octaves, persistence, offset)
- `SplatHeights` - Texture layer config (height range, slope range, noise params, tiling)
- `Vegetation` - Tree prototype config (mesh, height/slope constraints, color, scale, density)
- `Detail` - Detail layer config (mesh or billboard, height/slope constraints, colors, scale, density)

### Shared Systems (`Assets/SharedAssets/`)

**Player/Controller** (`FirstPersonController/`)
- `FirstPersonController.cs` - CharacterController-based FPS movement with Cinemachine
- `FirstPersonInputs.cs` (StarterAssetsInputs) - Input System wrapper
- `PlayerManager.cs` - Flythrough mode, mobile touch canvas activation

**Quality/Platform** (`Scripts/Runtime/`)
- `QualityInitialization.cs` - Auto-selects quality level
- `QualityLevelToggle.cs` - Quality-based GameObject toggling
- `GraphicsStateCollectionManager.cs` - PSO tracing/warmup singleton

**Editor Tools** (`Scripts/Editor/`)
- `HeightMapRenderer.cs` - Orthographic heightmap export to PNG
- `GradientTexture.cs` - ScriptableObject for gradient-to-texture generation

### Project Structure

```
Assets/
├── Scripts/
│   ├── CustomTerrain.cs         # Core terrain generation
│   └── Utils.cs                 # FBM helper
├── Editor/
│   ├── CustomTerrainEditor.cs   # Custom inspector
│   ├── TerrainEditorUtility.cs  # Menu commands
│   └── TerrainTestMenu.cs       # Quick test menu
├── Scenes/Main/
│   ├── MainScene.unity          # Primary scene
│   └── Documentation/           # Design principles
├── Settings/
│   ├── PC/, Mobile/             # Platform-specific render settings
│   └── VolumeProfiles/          # URP volume profiles
├── TerrainLayers/               # Auto-generated terrain layer assets
└── SharedAssets/
    ├── FirstPersonController/   # FPS controller + mobile input
    └── Scripts/                 # Quality, PSO, utility scripts
```

### Quality Levels

4 presets: Mobile Low, Mobile Medium, Mobile High, PC (default). Each has separate URP renderer in `Assets/Settings/`.

### Key Namespaces

- `StarterAssets` - FirstPersonController, StarterAssetsInputs
- Global namespace - CustomTerrain, Utils, quality utilities

## Design Principles

Procedural generation follows 5 principles (see `Assets/Scenes/Main/Documentation/DesignPrinciples.md`):
1. **Contrast** - Vary heights/colors, create focal points
2. **Repetition** - Limited asset variety, consistent reuse
3. **Alignment** - Place by altitude, slope, water distance
4. **Proximity** - Group similar objects, consider neighbors
5. **Coherence** - Matching asset sets, consistent color palette

## Key Packages

- `com.unity.render-pipelines.universal` (17.3.0) - URP rendering
- `com.unity.cinemachine` (3.1.2) - Camera control
- `com.unity.inputsystem` (1.17.0) - New Input System
- `com.unity.visualeffectgraph` (17.3.0) - VFX
- `com.unity.splines` (2.8.2) - Path/curve tools
- `com.unity.terrain-tools` (5.3.1) - Extended terrain editing
- `com.unity.test-framework` (1.6.0) - Unit testing
- `com.coplaydev.unity-mcp` - MCP integration for Claude Code

## Required Asset Store Packages

These packages are excluded from version control (see `.gitignore`) and must be installed manually:

- **Editor GUI Table** - Table UI for custom editors (used by CustomTerrainEditor)
  - URL: https://assetstore.unity.com/packages/tools/gui/editor-gui-table-108795
  - Install via Package Manager > My Assets
- **BasicTerrainAssets** / **TerrainSampleAssets** - Optional terrain textures and models

## Development Notes

### Terrain Generation Workflow
1. Add `CustomTerrain` component to a Unity Terrain GameObject
2. On first run in editor, auto-creates "Terrain" tag and layer
3. Use inspector foldouts or menu commands to generate terrain
4. `resetTerrain` toggle: true = start from flat, false = add to existing heights
5. `SplatMaps()` creates TerrainLayer assets in `Assets/TerrainLayers/`
6. `PlantVegetation()` uses raycasting against the Terrain layer for accurate tree placement

### Technical Notes
- PSO collections in `SharedAssets/GraphicsStateCollections/` are auto-generated per platform/quality combo
- Mobile builds activate touch input canvas automatically via `PlayerManager.Start()`
- Scene must include Camera and Directional Light (use `manage_scene` MCP tool when creating new scenes)
- Terrain algorithms use `GetHeightMap()` helper that respects `resetTerrain` flag

## PR Review Workflow

This project uses a two-AI review workflow:

1. **Create PR** - Claude Code creates the PR with summary and test plan
2. **Gemini Review** - Gemini (via GitHub) reviews the PR and posts comments
3. **Address Feedback** - Claude Code:
   - Implements code changes for valid suggestions
   - Posts replies to each comment explaining what was done
   - Uses `gh api` to reply to specific comment IDs
4. **Iterate** - Repeat steps 2-3 until approved

### Replying to PR Comments

```bash
# Reply to a specific review comment
gh api repos/{owner}/{repo}/pulls/{pr}/comments/{comment_id}/replies \
  -f body='Response message here'
```

### Comment Reply Guidelines
- Acknowledge valid points
- Explain what was implemented
- Reference specific changes (e.g., "Added constant", "Refactored to use X")