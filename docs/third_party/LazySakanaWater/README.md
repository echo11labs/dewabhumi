# LazySakana Water System 1.0.0

Water system by [LazySakana](https://lazysakana.co.in).

Free commercial use requires retaining this credit in distributed credits or accompanying documentation. The full custom attribution licence is retained at `Assets/Water/Documentation/LICENSE.txt`, alongside the vendor credit. This is not MIT or CC0.

Official source: https://github.com/SakanaLazy/Water_Material_Experimentation

Release: https://github.com/SakanaLazy/Water_Material_Experimentation/releases/tag/v1.0.0

The Complete package SHA-256 is `6f328f4cc2121157246e04dc6c105cc31bbb27ad7e87b3e6a72dff270f948336`, verified against the publisher's GitHub release digest. `manifest.json` records every retained file hash. Original paths and GUIDs are preserved. No scenes, model assets or Unity Package Manager caches were imported.

Integration entry point: **Dewabhumi > Landscape > Test Free Water System** (Ctrl+Shift+F6), starting from the geographic landscape scene. The integration creates `Assets/Scenes/GopalaKathmanduWater.unity` and assigns a tuned copy of the Realistic Plane preset to the nearest mapped Bagmati reach. Other rivers retain the previous material for comparison. It enables depth and opaque texture overrides on that preview camera only.

The vendor's **Water > Set Up Active URP Renderer** command is not used for this surface-only evaluation: it also assigns layer 30 and adds volume renderer features, which are unnecessary here. Renderer assets and project layers remain unchanged. Do not use volume presets on the existing open river meshes.

This package supplies rendering, not river routing, terrain carving, water discharge, flooding, buoyancy or navigation. The tuned flow direction is an artistic preview direction, not a spatially varying hydrological velocity field. Our river source data and mesh geometry remain authoritative for the route.

Before publishing or sharing a build, include the LazySakana credit above in the shipped credits/documentation.
