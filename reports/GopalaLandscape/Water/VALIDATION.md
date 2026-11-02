# LazySakana water compatibility check — 2026-11-02

- Installed official LazySakana Water System 1.0.0 Complete release. Verified publisher release SHA-256 before unpacking; all 136 admitted files matched the recorded package roster before Unity import.
- Inspected package scripts before admission. No InitializeOnLoad/automatic setup hook found. Did not invoke the vendor's layer/renderer-modifying setup command.
- Added a separate scene: `Assets/Scenes/GopalaKathmanduWater.unity`.
- Applied a tuned copy of the Realistic Plane preset to `Bagmati - way/518770057`, the mapped reach nearest the prototype camp. Its nearest mesh vertex is approximately 8.25 m from the camp coordinates.
- Enabled depth and opaque texture overrides only on the preview camera. No closed-volume renderer feature or layer change was required for the plane surface.
- C# and water Shader Graph imported/compiled in Unity 6000.6.7f1, URP 17.6.0, Windows DX12. Captures at settlement, valley, riverbank and surface views show the new shader rendering instead of an unsupported/pink material.
- Entered Play Mode, selected C close view, and observed animated surface detail/foam. Console showed zero errors; one unrelated pre-existing Unity Account API timeout warning appeared. Subsequent user window input prevented a confirmed final mode change; the saved preview scene and captures are available independently of the current Editor mode.
- Hash comparison confirmed the original landscape scene, URP pipeline asset, renderer asset, TagManager, GraphicsSettings and QualitySettings are unchanged from the pre-integration baseline (`baseline-hashes.json`).
- Original package presets, geographic source data, terrain heightmaps and river meshes were not modified by this integration. Other reaches deliberately retain the previous shader for comparison.
- Licence and copyright notice retained at `Assets/Water/Documentation`; required commercial credit added to root `CREDITS.md` and integration documentation.

## Remaining work

This is an isolated rendering evaluation, not a complete network conversion. Existing tributary meshes overlap the main river and need junction trimming/height continuity work before applying one consistent material across the network. River flow direction is a single artistic direction in the tuned material, not a bend-following velocity field. Bank shallows/depth profiles still need geometry refinement to make depth-based shoreline effects convincing. Hydraulic simulation, flooding, buoyancy, underwater views and performance targets were not implemented or certified.
