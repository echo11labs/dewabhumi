# Terrain texture pass — 2026-11-02

Implemented six 2K ambientCG CC0 material sets: Grass004, Ground037, Ground054, Ground048, Rock030 and Grass003. All 30 retained source image hashes matched the recorded receipt; official archive CRC checks passed during extraction. Source attribution, URLs, archive hashes and per-file hashes are retained in the source manifest.

Unity imported color as sRGB, OpenGL normals as Normal Maps, and packed masks as linear data. The owned URP Terrain Lit material has normal, mask and per-pixel terrain-normal keywords enabled. All six TerrainLayer assets reference their corresponding color, normal and packed mask maps. Source maps are 2K, mipmapped and anisotropically filtered.

Applied through the dedicated texture menu in the running Unity Editor. Heightmaps and river meshes were not regenerated. The local terrain control map increased to 1024 square; slope, river-edge distance and restrained macro variation drive material coverage. The detailed/basemap transition was moved outside the normal local view to remove the patch boundary exposed in the initial PBR render.

Inspected final settlement and surface captures. Added C riverbank and V surface presets and verified V in Play Mode. Final Play Mode Console showed zero errors and zero warnings; earlier editing logs included the existing citizen API deprecations and an unrelated Unity Account API connectivity warning. No new texture/script errors appeared.

Before renders: `TextureComparison/before-settlement.png`, `TextureComparison/before-ground.png`.
After renders: `settlement.png`, `ground.png`, `surface.png`, `valley.png`.

This is a first PBR surface pass. Texture scale and mipmapping naturally reduce visible fine detail at regional distances. Some tiling and the coarse source topography remain; no stochastic terrain shader, displacement, vegetation geometry, rock models or performance certification was added. No model assets were generated, and no new Unity packages or project-wide render settings were installed/changed.
