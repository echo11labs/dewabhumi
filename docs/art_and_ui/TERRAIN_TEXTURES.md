# Free terrain materials

The Gopala landscape uses six free ambientCG PBR texture sets by Lennart Demes, under CC0 1.0. Source files, source URLs, physical-scale notes and SHA-256 receipts are retained in `Assets/Art/Environment/TerrainTextures/ambientCG`. No downloaded code or models are used.

| Preview layer | Material | Tile width | Treatment |
| --- | --- | --- | --- |
| Meadow | [Grass004](https://ambientcg.com/view?id=Grass004) | 1.4 m | Short green ground cover |
| WoodlandFloor | [Ground037](https://ambientcg.com/view?id=Ground037) | 2.1 m | Mossy woodland soil |
| Alluvium | [Ground054](https://ambientcg.com/view?id=Ground054) | 3.5 m | Sandy, muddy river sediment |
| WetMeadow | [Ground048](https://ambientcg.com/view?id=Ground048) | 1.4 m | Damp soil near banks and scattered soil patches |
| ExposedRock | [Rock030](https://ambientcg.com/view?id=Rock030) | 3 m, artistic | Steep exposed ground |
| DryGrass | [Grass003](https://ambientcg.com/view?id=Grass003) | 1.4 m | Warmer, drier tint |

These are artistic surface choices, not surveyed Kathmandu plant communities or period-specific geology. The asset licence permits commercial use and redistribution; credit is optional. See [ambientCG licence](https://docs.ambientcg.com/license/).

## Rendering

- Each material uses 2K color, OpenGL normal, ambient occlusion, roughness and displacement inputs.
- Color is imported in sRGB. Normals use Unity's Normal Map importer. Packed masks are linear: R = zero metallic, G = occlusion, B = height, A = one minus roughness.
- Height is retained in the mask for future authoring; it does not displace the geographic terrain. URP height blending stays disabled because it supports at most four terrain layers, while this terrain uses six.
- Trilinear mipmapping, anisotropic filtering and physical-scale tiling preserve detail at oblique camera angles. Surface masks use broad and fine noise, elevation, slope and distance from the river edge; the local control map is 1024 square. Detailed shading remains active to 10 km to keep its transition outside the normal settlement view.
- The project owns `TerrainSurface.mat`; shared package materials and project-wide renderer settings are not edited.
- Full landscape rebuilds retain the PBR setup once the source manifest is present. The procedural textures remain an offline fallback if the downloaded material set is absent.

## Reproduce and inspect

1. Run `python tools/fetch_terrain_textures.py` to fetch the six official archives. Downloads are restricted to named image maps, checked using ZIP CRC and recorded with SHA-256 hashes.
2. Open `Assets/Scenes/GopalaKathmandu.unity` in Unity.
3. Run **Dewabhumi > Landscape > Apply Free PBR Terrain Textures** (Ctrl+Shift+F7). This updates surfaces and control maps without regenerating heightmaps or rivers.
4. Enter Play Mode: C frames the riverbanks; V frames the surface at close range. Wheel zoom can reach 25 m camera distance. O and L retain overview/settlement navigation.

The capture command writes `surface.png` in addition to settlement, valley and ground images. Earlier renders are retained under `reports/GopalaLandscape/TextureComparison`.

Texture detail improves ground shading but cannot replace supplied vegetation/rock meshes, high-resolution local terrain sculpting or lighting polish. Regional elevation remains limited by its source data.
