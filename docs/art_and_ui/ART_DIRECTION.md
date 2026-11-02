# DEWABHUMI — Art Direction & Visual Identity

> **Aesthetic Pillars, Camera Projection, Lighting, Palette, and Readability**

---

## 1. Visual Pillars

```
+---------------------------------------------------------------------------------+
|                               DEWABHUMI AESTHETICS                              |
+------------------------+-------------------------------+------------------------+
| 1. HANDCRAFTED         | 2. EVOCATIVE HIMALAYAN        | 3. SILHOUETTE          |
|    ORGANIC WARMTH      |    ATMOSPHERE                 |    LEGIBILITY          |
| Weathered wood, carved | Drifting mountain mists, deep | Instant clarity at     |
| struts, terracotta mud | green terrace fields, and     | settlement zoom; no    |
| plaster, and hay.      | crisp golden sunlight.        | visual clutter.        |
+------------------------+-------------------------------+------------------------+
```

---

## 2. Camera & Projection Contract

- **Projection Mode:** 2.5D Orthographic / Low-FOV Perspective (Isometric feel).
- **Fixed Heading:** Fixed azimuth angle ($45^\circ$ cardinal angle) to ensure buildings, silhouettes, and shadow maps remain completely consistent and readable.
- **Pitch Angle:** $30^\circ$ to $35^\circ$ decline from horizontal, matching classic 2:1 isometric ground projection.
- **Zoom Range:**
  - **Close Inspection (5m – 10m):** Inspect individual villager clothing, carrying baskets, cattle horns, and garden vegetables.
  - **Settlement Management (20m – 35m):** Default strategic view for construction, path laying, and worker monitoring.
  - **Valley Overview (60m – 100m):** Broad geographic perspective showing river meanders, mountain backdrop, and regional weather fronts.

---

## 3. Color Palette & Lighting

| Palette Element | Color Tone | Hex Code | Material & Emotional Context |
|---|---|---|---|
| **Terracotta Earth** | Rich Ochre Red | `#A84A2A` | Clay bricks, roof tiles, sun-baked fertile mud terraces. |
| **Aged Timber** | Deep Walnut / Teak | `#4A3525` | Hand-carved Sal beams, roof struts, timber fencing. |
| **Valley Grass** | Vibrant Spring Green | `#5A8235` | Lush alpine pastures, young rice paddy terraces. |
| **Monsoon Slate** | Cool Blue-Grey | `#4B5E68` | Riverbed granite boulders, rain-slick stone pavements. |
| **Sacred Brass** | Warm Weathered Gold | `#D4A24E` | Temple roof finials, ceremonial bells, butter lamps. |
| **Himalayan Mist** | Soft Pearl White | `#E8ECEF` | Morning valley fog, distant glacial snow peaks. |

### Unity URP Lighting Setup
- **Main Light:** Warm directional sunlight ($5200K$ color temperature) casting soft, defined shadows toward the northwest.
- **Ambient Fill:** Cool sky ambient ($7000K$) reflecting off the Himalayan mountain ridge backdrop.
- **Post-Processing Volume:** Subdued ACES tonemapping, light bloom on glistening water surfaces, and subtle depth of field on overview zoom.
