# Landscape validation — 2026-11-02

- Unity 6000.6.7f1 compiled the added C# scripts and URP river shader; final generation showed zero Console warnings and zero errors.
- Executed the geographic builder in the running Unity Editor and generated `Assets/Scenes/GopalaKathmandu.unity`.
- Verified all six retained upstream input checksums against the pinned source receipt.
- Region: 40 x 40 km, native elevation sampling 39.0625 m. Local tile: 5 x 5 km, interpolated/procedural sampling 4.8828125 m.
- 203 visible mapped river/stream reaches generated. Covered and underground reaches omitted; no inferred headwater promoted to mapped data.
- Local/regional boundary sample difference: 0 m. Height normalization did not clip.
- Inspected `settlement.png`, `valley.png` and `ground.png`. Corrected visible ground-texture repetition, local water burial, and generated-asset naming warnings found during iteration.
- Entered Play Mode. The O overview and C close-view keys visibly changed the camera. The overlay rendered, and no runtime Console errors or warnings appeared during this smoke check. Returned to local view and stopped Play Mode.
- Original dirty Untitled scene preserved at `Assets/_Recovery/BeforeGopalaLandscape.unity`. MainSettlement and Build Settings retained.
- No character, building, vegetation, rock or prop models generated. No new packages installed; used the existing URP pipeline and Unity Editor through Computer Use.

## Limits

This is a terrain-and-water foundation, not a finished historical environment. Ground detail below the source resolution is artistic, and the modern DSM still contains modern surface signals. Coarse regional waterways can intersect terrain away from the detailed local tile and require further regional river/LOD refinement. Water has animated surface shading but no hydraulic, flood, reflection-probe or underwater simulation. Vegetation and bank-dressing models are awaited from the user. Settlement gameplay, navigation, builds, performance targets and exact Gopala-era geography were not validated.
