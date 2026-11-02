# Gopala landscape reconstruction

The preview is an evidence-aware early pastoral interpretation, not an exact archaeological reconstruction or a dated map of the Gopala dynasty. No exact reign dates, ancient channels, settlement locations or period-specific vegetation distribution are asserted.

## Geographic foundation

Inputs come from [mandalaverse](https://github.com/echo11labs/mandalaverse/tree/e95b57c51e3a8e292415405e2d4b4577f3462d3b/game/data/geography), pinned to commit `e95b57c51e3a8e292415405e2d4b4577f3462d3b`. The source receipt verifies SHA-256 checksums. Original inputs and attribution live in `Assets/Geography/Kathmandu`.

- Extent: 40 x 40 km, centered at 27.695 N, 85.375 E; projection EPSG:32645.
- Samples: 1025 x 1025, spaced 39.0625 m. Copernicus GLO-30 supplies broad landforms.
- Unity coordinates: X east, Z north, Y elevation above sea level, one unit per metre. Source raster rows run north to south and are reversed during sampling. No vertical exaggeration is applied.
- Source and cleaned visual elevation remain separate. The upstream cleaned surface has median filtering and channel dressing; it is not a recovered ancient bare-earth surface.
- Modern mapped waterways are geographic proxies. Covered/underground reaches are excluded. The separate speculative Bagdwar reconstruction is not imported as a mapped river.

Copernicus is a modern **surface model**, including vegetation and infrastructure. Omitting modern roads/building models does not remove their elevation signal from the DSM. A higher-quality bare-earth reconstruction would require additional terrain data and reviewed local corrections. See [Copernicus dataset description](https://registry.opendata.aws/copernicus-dem/).

## Interpretation choices

The local 5 x 5 km terrain tile near the upstream prototype's camp replaces an exact 128 x 128-cell region of the wider terrain. Its 1025-sample heightmap interpolates the source and adds restrained sub-metre variation, local riverbed dressing and a small gently sloping clearing. Finer sampling is an authoring surface, **not new measured detail**. The campsite is an artistic choice, not an excavated settlement.

Meadow, woodland floor, alluvium, wet soil, exposed rock and dry grass now use free 2K CC0 ambientCG PBR textures, replacing the first-pass procedural placeholders. Normal maps and packed occlusion/roughness/height masks supply surface detail. Slope, elevation, river distance and broad/fine noise patches determine surface blends. These masks are art direction, not paleovegetation evidence. No old isometric tile images are reused as terrain textures. See `docs/art_and_ui/TERRAIN_TEXTURES.md` for sources and reproduction; procedural surfaces remain a fallback when the texture manifest is absent.

River display widths are artistic: Bagmati 36 m, selected major tributaries 20 m, other mapped rivers 12 m, streams 6 m. The water surface follows the prepared ground profile, with a small visual lift where necessary to avoid terrain occlusion. An independent URP shader animates surface ripples and shore highlights; it is not a discharge, flood or hydraulic simulation. Modern river alignments do not establish exact Gopala-era meanders.

No basin-wide lake is invented for this setting: the published ancient-lake evidence concerns Late Pleistocene processes, not a dated Gopala-period shoreline. See [Saijo and Kimura, Expansion of an ancient lake in the Kathmandu basin](https://www.nepjol.info/index.php/HJS/article/view/981) and [Sunakothi Formation study](https://doi.org/10.3126/jngs.v39i0.31486). Dynastic chronology must be treated separately from landscape geology; the [Gopalarajavamsavali edition](https://www.si.edu/object/siris_sil_1098335) is a textual source, not a terrain survey.

No later temples, stupas, urban roads, mass engineered terraces or medieval civic structures are added as evidence for an early pastoral landscape.

## Authoring and controls

1. Fetch/verify inputs with `python tools/fetch_kathmandu.py`.
2. Convert metadata with `python tools/prepare_kathmandu.py`.
3. In Unity, run **Dewabhumi > Landscape > Build Gopala Kathmandu Preview** (Ctrl+Shift+F8).
4. Open `Assets/Scenes/GopalaKathmandu.unity`. Build Settings remain unchanged.
5. Use **Capture Gopala Preview** (Ctrl+Shift+F9) for images under `reports/GopalaLandscape`.

The builder preserves dirty open scenes as uniquely named copies in `Assets/_Recovery` before switching. It writes only its dedicated generated assets and preview scene; the old MainSettlement scene is retained. Rebuilds update existing generated asset contents while preserving GUIDs. The old CompleteSceneFixer is now explicit-menu-only; it no longer opens/rebuilds scenes on a domain reload.

In Play Mode: O = valley overview; L/Home = settlement; C = riverbanks; V = close surface view; WASD/middle-drag = pan; wheel = zoom; right-drag or Q/E = orbit; Shift = faster movement.

## Models and next art pass

Per user instruction, no character, building, vegetation, rock or prop models are generated. Supply broadleaf trees, riparian shrubs, grasses/reeds and river rocks for the vegetation and bank-dressing pass. Terrain and river surfaces are procedural landscape geometry. The preview intentionally has no substitute tree/rock models.

## Validation

The importer verifies upstream checksums, raster sizes and finite elevation values. The builder rejects clipped elevations and checks local/regional boundary agreement. `reports/GopalaLandscape/build.json` records generation metrics; renders provide visual evidence. The preview has been generated in Unity, shader rendering inspected at three distances, and boundary error measured at zero. Overview and close-view controls passed a Play Mode smoke check without runtime Console errors. These checks do not establish historical accuracy or settlement gameplay/navigation readiness. Current limitations and validation evidence are recorded in `reports/GopalaLandscape/VALIDATION.md`.

Elevation and OSM data retain their separate upstream attribution and licences. The metadata conversion does not relicense those datasets.
