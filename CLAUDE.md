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

Run tests via Unity Test Framework (Edit Mode tests available).

## Architecture

### Core Systems

**Player/Controller** (`Assets/SharedAssets/FirstPersonController/`)
- `FirstPersonController.cs` - CharacterController-based FPS movement with Cinemachine camera integration
- `FirstPersonInputs.cs` (StarterAssetsInputs) - Input System wrapper with idle detection
- `PlayerManager.cs` - Flythrough mode, device-specific UI switching, handles mobile touch canvas activation

**Quality/Platform Scaling** (`Assets/SharedAssets/Scripts/Runtime/`)
- `QualityInitialization.cs` - Auto-selects quality level (falls back to PC Low for OpenGL)
- `QualityLevelToggle.cs` - Enables/disables GameObjects based on quality level bitmask
- `GraphicsStateCollectionManager.cs` - Singleton for PSO (Pipeline State Object) tracing/warmup per platform+quality combo

**Editor Tools** (`Assets/SharedAssets/Scripts/Editor/`)
- `HeightMapRenderer.cs` - Renders orthographic height maps to PNG via temporary camera
- `GradientTexture.cs` - ScriptableObject that generates textures from gradients
- `NamingConvention.cs` - Batch object renaming utility
- `GraphicsStateCollectionCombiner.cs` / `GraphicsStateCollectionStripper.cs` - PSO collection management for builds

### Project Structure

```
Assets/
├── Scenes/Main/
│   ├── MainScene.unity          # Primary scene
│   └── Documentation/           # Design docs
├── Settings/
│   ├── PC/, Mobile/             # Platform-specific render settings
│   └── VolumeProfiles/          # URP volume profiles
└── SharedAssets/
    ├── FirstPersonController/   # FPS controller + mobile input
    ├── Scripts/Runtime/         # Quality, PSO, utility scripts
    ├── Scripts/Editor/          # Editor tools
    ├── Prefabs/                 # FPS_Controller_Prefab, RuntimeDataCanvas
    └── GraphicsStateCollections/ # PSO cache files (auto-managed)
```

### Quality Levels

4 quality presets: Mobile Low, Mobile Medium, Mobile High, PC (default). Each has separate URP renderer settings in `Assets/Settings/`.

### Key Namespaces

- `StarterAssets` - FirstPersonController, StarterAssetsInputs
- Global namespace - PlayerManager, quality/graphics utilities

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
- `com.coplaydev.unity-mcp` - MCP integration for Claude Code

## Notes

- PSO collections in `SharedAssets/GraphicsStateCollections/` are auto-generated per platform/quality combo. Use context menu "Update collection list" on GraphicsStateCollectionManager to refresh.
- Mobile builds activate touch input canvas automatically via `PlayerManager.Start()`.
- Scene must include Camera and Directional Light. Use `manage_scene` MCP tool when creating new scenes.