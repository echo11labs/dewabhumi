# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project summary

- Verified root: `F:/Game-Development/Dewabhumi` (Assets, manifest and ProjectVersion present).
- Analyzed: 2026-11-02. Commit: unavailable; this folder is not a Git repository.
- Dewabhumi is a Kathmandu Valley settlement survival simulation prototype. Design intent is documented in `docs/README.md`, `docs/GDD.md` and the design suite. Treat roadmap completion claims as plans unless verified in code.
- Inspection used repository files only. No Unity assets, settings or packages were changed; no Editor launch, Play Mode, build or tests were triggered.

## Confirmed environment

| Area | Finding | Evidence |
| --- | --- | --- |
| Editor | Unity 6.6, 6000.6.7f1, revision b22767f04f04 | ProjectSettings/ProjectVersion.txt |
| Rendering | URP 17.6.0, configured default pipeline points to Dewabhumi_URP.asset | Packages/manifest.json; ProjectSettings/GraphicsSettings.asset; Assets/Settings/Dewabhumi_URP.asset.meta |
| Input | Both backends enabled (activeInputHandler 2); inspected gameplay uses legacy UnityEngine.Input | ProjectSettings/ProjectSettings.asset; WorldViewCamera.cs; SettlementHUDManager.cs; BlueprintPlacementController.cs |
| UI | Current HUD uses IMGUI OnGUI; UI Toolkit is the documented target | SettlementHUDManager.cs; docs/art_and_ui/UI_CONTRACT.md |
| Platforms | Desktop-first design; Steam Deck appears in roadmap. Actual build target and player support unverified | docs/README.md; docs/roadmap/ROADMAP.md |

## Important packages and frameworks

- Confirmed direct packages: URP 17.6.0, Input System 1.20.1, AI Navigation 2.0.16, Splines 2.9.1; Rider and Visual Studio integration.
- Lock file contains builtin and Unity-registry sources only; no Git/local/custom package sources found. No embedded package or Unity MCP package found.
- Unity Test Framework 1.8.0 is present transitively in packages-lock.json (not a direct manifest dependency).
- No networking framework or first-party networking usage found. AI Navigation installation does not establish its use: inspected citizens/cattle move transforms directly.
- No confirmed Addressables, localization, Entities, DI or async framework dependencies.

## Directory structure

| Path | Responsibility |
| --- | --- |
| Assets/Scripts/Core | Pure C# simulation clock and entity ID registry |
| Assets/Scripts/Citizens; Pastoral | Citizen needs/actions, population management and cattle |
| Assets/Scripts/Economy | Inventory, storage, resource nodes and harvesting |
| Assets/Scripts/Settlement; Spatial | Building definitions, construction, dwellings, occupancy and 2m coordinate utilities |
| Assets/Scripts/Presentation | Camera, IMGUI HUD, grid overlay |
| Assets/Scripts/Environment | Procedural Kathmandu terrain; Editor subfolder contains scene creation/rebuild tools |
| Assets/Scripts/Settlement/Editor | Building-definition authoring tool |
| Assets/Art | Buildings, Characters, Environment, Materials and Props, including imported models |
| Assets/Settings | URP pipeline and renderer assets |
| Assets/Scenes; Assets/_Recovery | MainSettlement and recovery scene respectively |
| Assets/UI | Placeholder Templates, Styles and Icons directories; no UXML/USS files found |
| docs | Design contracts, historical constraints, art/UI specifications and roadmap |

## Assembly boundaries

No .asmdef or .asmref files found under Assets or Packages. First-party runtime scripts use the default Assembly-CSharp; Editor-folder scripts use Assembly-CSharp-Editor. Feature folders and namespaces organize code, but there are no custom assembly boundaries enforcing simulation/presentation isolation. Unsafe code and platform-specific behavior were not exhaustively audited.

## Scenes and startup flow

- Confirmed only enabled build scene: `Assets/Scenes/MainSettlement.unity` (EditorBuildSettings.asset); likely runtime entry scene. Recovery scene is not enabled for builds.
- Both scene files are binary serialized; current hierarchy, missing references and manager instances were not verified. EditorSettings currently requests text serialization, which has not converted these existing files.
- Code-confirmed runtime flow: SettlementPopulationManager.Awake creates SimulationClock and EntityRegistry; Start subscribes tick propagation and optionally spawns eight citizens and two cattle. Update drives the 10 Hz clock; ticks reach citizens and cattle. Storage and harvesting managers have their own startup spawning paths.
- Editor-only scene authoring: CompleteSceneFixer builds terrain, camera rig, settlement anchor, managers, placement and HUD, then saves/reopens the scene. It now runs only by explicit menu action.
- New geographic preview: `Assets/Scenes/GopalaKathmandu.unity`, generated through `Dewabhumi > Landscape > Build Gopala Kathmandu Preview`. Real 40 km regional elevation, a 5 km detailed local tile, 203 visible mapped river reaches, fresh procedural ground textures and an animated URP river shader. `KathmanduLandscapeCamera` supplies independent preview navigation. Build Settings still use MainSettlement. See `docs/world/GOPALA_LANDSCAPE.md` for data provenance and interpretation limits.
- Texture follow-up: six 2K ambientCG CC0 PBR sets replace the preview's procedural surface placeholders, with normal maps, packed masks and a 1024-square local control map. Sources and hashes: `Assets/Art/Environment/TerrainTextures/ambientCG/manifest.json`. Apply without rebuilding geometry through `Dewabhumi > Landscape > Apply Free PBR Terrain Textures`. C/V now frame bank/surface detail. See `docs/art_and_ui/TERRAIN_TEXTURES.md`.
- Water follow-up: LazySakana Water System 1.0.0 is imported with original GUIDs and licence notices. `GopalaKathmanduWater.unity` is an isolated rendering test on the nearest mapped Bagmati reach, using a tuned Realistic Plane material. Only that preview camera requests depth/opaque textures; project render settings/layers and the original landscape scene remain unchanged. Menu: `Dewabhumi > Landscape > Test Free Water System`. Details and receipt: `docs/third_party/LazySakanaWater`; commercial credit: `CREDITS.md`. Existing tributary overlaps still require geometry work.
- No runtime scene-loading system found in inspected scripts.

## Architecture and conventions

- Confirmed hybrid architecture: pure C# clock, registry and inventory helpers combined with MonoBehaviour entities and singleton managers. BuildingDefinition is a ScriptableObject.
- Confirmed direct cross-feature dependencies: citizens consult settlement/economy managers; presentation reads managers directly. Simulation authority is not yet isolated from Unity objects as required by the design document.
- Confirmed citizens/cattle movement runs in Update with Time.deltaTime; cattle wandering uses UnityEngine.Random. A fixed tick clock alone does not establish deterministic whole-game behavior.
- Common conventions across representative scripts: block-scoped Dewabhumi.<Feature> namespaces, Allman braces, private _camelCase fields, [SerializeField] private authoring fields, PascalCase public members and XML summaries. No explicit formatting policy or established async convention found.
- Save/load is specified in design documentation; a complete persistence implementation was not found in the inspected source set. Clock.SetState is not evidence of full save/load support.

## Testing and validation

No first-party EditMode/PlayMode tests, test assemblies, CI configuration or build scripts found in the inspected non-generated project files. Test Framework is available through the lock file, but Editor test discovery and compilation status remain unverified. No tests or builds were run during onboarding.

## Available Unity tooling

- Unity MCP connection, live version, console, scene inspection, build-settings read, GameObject inspection, asset/package search, tests, Play Mode state and profiler: unavailable through the tools exposed in this session.
- No provider found in package manifests or project configuration filenames. Live Editor state remains unverified. Repository-based work can continue without installing a provider.
- Landscape follow-up: Unity was operated through the Computer Use plugin; the new preview compiled and generated in the running Editor, with rendered captures under `reports/GopalaLandscape`. No Unity MCP or new package was installed. The earlier live-state limitation applies to the original settlement inspection.
- Generated .csproj files and Dewabhumi.slnx exist; they are not authoritative package or architecture evidence.

## Important constraints and risks before changes

1. Preserve the material conservation invariant, simulation/presentation boundary intent and historical-fiction boundary in the documentation. Current implementation differs from the intended architecture; do not assume design prose describes completed behavior.
2. CompleteSceneFixer's automatic scene-opening/rebuilding hook was removed during landscape work. Its Setup Main Settlement Scene and Fix & Rebuild menus still reconstruct the old scene explicitly. Do not invoke these actions merely to inspect. The geographic builder writes a separate preview and saves dirty scenes to unique recovery copies before switching.
3. No Git baseline is available in this folder. Establish a recoverable baseline before substantive changes.
4. Binary scenes limit repository review. Current scene wiring, compile health, runtime behavior, build readiness, save/load completeness and resource conservation are unknown pending focused validation.

## Source files inspected

ProjectSettings/{ProjectVersion.txt,EditorBuildSettings.asset,GraphicsSettings.asset,QualitySettings.asset,ProjectSettings.asset,EditorSettings.asset}; Packages/{manifest.json,packages-lock.json}; Assets/Settings/Dewabhumi_URP.asset.meta; docs/{README.md,design/SIMULATION_ARCHITECTURE.md,art_and_ui/UI_CONTRACT.md,roadmap/ROADMAP.md}; Assets/Scripts/Core/{SimulationClock.cs,EntityRegistry.cs}; Citizens/{SettlementPopulationManager.cs,CitizenEntity.cs}; Economy/InventoryComponent.cs; Settlement/BuildingDefinition.cs; Environment/Editor/{SettlementSceneInitializer.cs,CompleteSceneFixer.cs}. Targeted source searches also covered input, movement, lifecycle, namespaces, serialization fields, scene loading, networking and persistence across Assets/Scripts. Scene headers were checked for serialization format only.

<!-- unity-onboarding:generated:end -->
