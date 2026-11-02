# Dewabhumi

A Kathmandu Valley settlement simulation prototype inspired by early pastoral traditions, built with Unity 6.6 and URP. The geographic landscape is an evidence-aware artistic interpretation; it is not an exact archaeological reconstruction of the Gopala period.

## Open the project

1. Install Git LFS, then clone this repository:

   ```sh
   git lfs install
   git clone https://github.com/echo11labs/dewabhumi.git
   cd dewabhumi
   git lfs pull
   ```

2. Open the repository folder in Unity Hub with **Unity 6000.6.7f1**.
3. Allow Unity to restore the packages recorded in `Packages/manifest.json` and import the assets.

`Library`, `Temp`, logs, IDE files, local download caches and recovery scenes are intentionally excluded.

## Scenes

- `Assets/Scenes/MainSettlement.unity`: original settlement prototype and current Build Settings entry.
- `Assets/Scenes/GopalaKathmandu.unity`: geographic landscape and free PBR terrain materials.
- `Assets/Scenes/GopalaKathmanduWater.unity`: isolated LazySakana water rendering test on a mapped Bagmati reach.

In either geographic preview, enter Play Mode and use **O** for the valley, **L/Home** for the settlement, **C** for riverbanks and **V** for surface detail. WASD/middle-drag pans, wheel zooms, and right-drag/Q/E rotates.

## Documentation and tools

- [Design documentation](docs/README.md)
- [Project context](docs/AI/UnityProjectContext.md)
- [Landscape provenance and reconstruction limits](docs/world/GOPALA_LANDSCAPE.md)
- [Free terrain material sources](docs/art_and_ui/TERRAIN_TEXTURES.md)
- [Water-system integration](docs/third_party/LazySakanaWater/README.md)

The `Dewabhumi > Landscape` Editor menu contains explicit generation, texturing, water-preview and capture commands. Source geographic data and material maps are included via Git LFS; the Python acquisition scripts under `tools` can reproduce their downloads and verify recorded checksums.

The water integration is an initial rendering evaluation. River junctions, shallow-bank geometry and full-network integration remain unfinished. Automated gameplay tests and a release build are not yet established.

## Credits and third-party licences

Water system by [LazySakana](https://lazysakana.co.in).

See [CREDITS.md](CREDITS.md), the retained Water System Attribution Licence, ambientCG CC0 material receipts, and Copernicus/OpenStreetMap geographic attribution. Their licences apply separately; no blanket licence is granted over third-party assets.
