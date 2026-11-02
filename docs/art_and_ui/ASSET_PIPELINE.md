# DEWABHUMI — Master 3D Asset Pipeline & Technical Specifications

> **Definitive Standards for Metric Sizing, Grid Footprints, Sockets, LODs, PBR Materials, and Quality Verification**

---

## 1. Core Modeling Principles & Coordinate Conventions

To guarantee that all newly created 3D assets align seamlessly into the simulation engine with zero rescale hacks or orientation bugs, every model must strictly conform to these baseline rules:

| Property | Rule | Enforcement & Verification |
|---|---|---|
| **World Unit Scale** | **1 Unity Unit = Exactly 1.0 Metre** | 100% 1:1 metric modeling. Never export with arbitrary unit conversions. |
| **Coordinate Axes** | **$+Y$ = Up**, **$+Z$ = Forward**, **$+X$ = Right** | Standard Unity coordinate system (Right-Handed in DCC software, exported as glTF/FBX with $+Y$ Up). |
| **Pivot Placement (Buildings)** | **$(X=0, Y=0, Z=0)$ at Absolute Bottom Center** | Ground contact point must sit precisely on $Y=0$. Even-dimension footprints ($2\times 2$, $4\times 4$) center on the grid intersection; odd dimensions center on cell centers. |
| **Pivot Placement (Characters)** | **$(0,0,0)$ at Ground Level between Feet** | Root bone at $(0,0,0)$, pelvis/hips parented appropriately. |
| **Pivot Placement (Props/Tools)** | **$(0,0,0)$ at Center of Base or Grip Point** | Grip tools align $(0,0,0)$ to the primary grip point for clean hand socket snapping. |
| **Scale Transform** | **Scale must be $(1.0, 1.0, 1.0)$** | Transforms must be frozen/applied in DCC before export (`Apply All Transforms`). |

---

## 2. Complete Metric Sizing & Footprint Matrix

Every new asset authored for **Dewabhumi** must conform to the following exact metric dimensions:

### A. Citizens & Fauna

```
       +Y (1.75m)
         /\
        /  \     [Head] (Top at 1.75m)
       | () |    [Eye Level: 1.62m]
        \__/
         ||
       /|  |\    [Shoulder Width: 0.50m]
      / |  | \   [Shoulder Height: 1.45m]
     /  |  |  \
        |  |     [Waist / Hip: 0.95m]
        |  |
        |  |     [Knee: 0.50m]
        /  \
       /    \
      |      |   [Footprint Base: 0.35m depth x 0.50m width]
======+======+====== Y = 0.00m (Ground Pivot)
```

| Entity | Height ($Y$) | Width ($X$) | Depth ($Z$) | Bounding Envelope | Socket Attachments |
|---|---|---|---|---|---|
| **Adult Settler (Male/Female)** | $1.75\text{ m}$ | $0.50\text{ m}$ | $0.35\text{ m}$ | $0.5\times 1.75\times 0.35\text{ m}$ | `Socket_Hand_R`, `Socket_Hand_L`, `Socket_Back_Dokoko`, `Socket_Head` |
| **Elder Settler** | $1.65\text{ m}$ | $0.48\text{ m}$ | $0.35\text{ m}$ | $0.48\times 1.65\times 0.35\text{ m}$ | `Socket_Hand_R`, `Socket_Hand_L` |
| **Youth / Child** | $1.20\text{ m}$ | $0.40\text{ m}$ | $0.28\text{ m}$ | $0.4\times 1.2\times 0.28\text{ m}$ | `Socket_Hand_R`, `Socket_Back_Basket` |
| **Zebu Cow (*Bos indicus*)** | $1.40\text{ m}$ (Shoulder) | $0.80\text{ m}$ | $2.20\text{ m}$ | $0.8\times 1.6\times 2.2\text{ m}$ (Hump at 1.6m) | `Socket_Mouth_Grazing`, `Socket_Neck_Bell` |
| **Zebu Bull** | $1.55\text{ m}$ (Shoulder) | $0.95\text{ m}$ | $2.40\text{ m}$ | $0.95\times 1.8\times 2.4\text{ m}$ (Prominent Hump) | `Socket_Yoke_Draft` |
| **Calf** | $0.85\text{ m}$ (Shoulder) | $0.45\text{ m}$ | $1.10\text{ m}$ | $0.45\times 0.95\times 1.1\text{ m}$ | — |

---

### B. Buildings & Structures (2m Grid Alignment)

All building models must match an exact integer multiple of the **$2.0\text{ m} \times 2.0\text{ m}$ logical grid cell**.

| Structure | Footprint Cells | Ground Dimensions ($X \times Z$) | Max Height ($Y$) | Door Opening ($W \times H$) | Foundation Pad |
|---|---|---|---|---|---|
| **Worker Hut / Simple Ghar** | $2 \times 3$ cells | $4.0\text{ m} \times 6.0\text{ m}$ | $3.8\text{ m}$ (Gable Ridge) | $1.0\text{ m} \times 2.0\text{ m}$ | Plinth height: $0.20\text{ m}$ dry stone |
| **Stone House (Upgraded)** | $3 \times 3$ cells | $6.0\text{ m} \times 6.0\text{ m}$ | $5.5\text{ m}$ (2-Story Ridge) | $1.2\times 2.1\text{ m}$ | Plinth height: $0.35\text{ m}$ carved masonry |
| **Clan Meeting Hall / Chautara**| $4 \times 4$ cells | $8.0\text{ m} \times 8.0\text{ m}$ | $6.5\text{ m}$ (Tiered Roof) | $1.6\times 2.2\text{ m}$ (Open portico) | Plinth height: $0.50\text{ m}$ stone terrace |
| **Dry Granary (*Bhakari*)** | $3 \times 4$ cells | $6.0\text{ m} \times 8.0\text{ m}$ | $4.5\text{ m}$ | $1.2\times 2.0\text{ m}$ | Raised on $0.60\text{ m}$ anti-rodent stilts |
| **Open Stockpile & Shed** | $2 \times 2$ cells | $4.0\text{ m} \times 4.0\text{ m}$ | $2.8\text{ m}$ (Lean-to roof) | Open front ($3.6\text{ m}$ bay) | Level packed dirt / gravel |
| **Cattle Barn (*Goth*)** | $4 \times 4$ cells | $8.0\text{ m} \times 8.0\text{ m}$ | $4.2\text{ m}$ | $2.2\times 2.2\text{ m}$ (Livestock gate) | Split-level with dry manure pit |
| **Watermill (*Pani Ghatta*)** | $3 \times 3$ cells | $6.0\text{ m} \times 6.0\text{ m}$ | $3.6\text{ m}$ | $1.0\times 1.9\text{ m}$ | Sub-floor flume channel: $1.2\text{ m}$ depth |
| **Dug Well (*Kuwa*)** | $1 \times 1$ cell | $2.0\text{ m} \times 2.0\text{ m}$ | $2.4\text{ m}$ (Frame/Beam) | Open circumference ($1.4\text{ m}$ ID) | Stone parapet height: $0.80\text{ m}$ |
| **Terraced Farmhouse** | $3 \times 4$ cells | $6.0\text{ m} \times 8.0\text{ m}$ | $5.2\text{ m}$ | $1.1\times 2.0\text{ m}$ | Stepped stone terrace foundation |
| **Bridge Module** | $1 \times 2$ cells | $2.0\text{ m} \times 4.0\text{ m}$ | $1.2\text{ m}$ (Handrails) | $1.6\text{ m}$ clear walkway | Abutments resting on river banks |

---

### C. Environment, Foliage & Natural Resource Nodes

| Asset | Height ($Y$) | Crown / Base Diameter | Interactive Yield | Visual Harvest Stages |
|---|---|---|---|---|
| **Mature Sal Timber Tree** | $16.0\text{ m}$ | Crown: $8.0\text{ m}$, Trunk: $0.70\text{ m}$ | 6 Timber Logs | Full Tree -> Felled Trunk -> 6 Cut Logs -> Stump |
| **Medium Alder (*Utis*) Tree** | $11.0\text{ m}$ | Crown: $5.0\text{ m}$, Trunk: $0.45\text{ m}$ | 4 Timber Logs | Full Tree -> Felled Trunk -> 4 Cut Logs -> Stump |
| **Sacred Banyan / Pipal Tree** | $18.0\text{ m}$ | Crown: $14.0\text{ m}$, Trunk: $2.40\text{ m}$ | Indestructible (Civic Anchor) | Sacred prayer flags around trunk |
| **Granite River Boulder** | $0.60\text{ m}$ | $0.90\text{ m} \times 0.75\text{ m}$ | 4 Loose Stone | Intact Boulder -> Fractured Rock -> Gravel Base |
| **Small Shingle Deposit** | $0.25\text{ m}$ | $1.40\text{ m}$ diameter patch | 2 Loose Stone | Cluster of 8-12 cobbles on riverbed |
| **River Reed Bed (*Kans*)** | $1.80\text{ m}$ | $2.0\text{ m} \times 2.0\text{ m}$ patch | 4 Thatch Bundles | Dense reeds -> Cut Stubble patch |
| **Wild Mountain Berry Bush** | $1.20\text{ m}$ | $1.50\text{ m}$ diameter | 4 Foraged Food | Fruiting bush -> Stripped leaf bush |

---

### D. Tools, Carried Loads & Props

| Prop Asset | Metric Dimensions ($X \times Y \times Z$) | Attachment Socket | Usage / Function |
|---|---|---|---|
| **Dokoko Carrying Basket** | $0.45\text{ m} \times 0.65\text{ m} \times 0.40\text{ m}$ | `Socket_Back_Dokoko` | Hauling woven bamboo basket with forehead strap (*Namlo*). |
| **Timber Log (Haulable)** | $0.30\text{ m} \text{ diameter} \times 2.0\text{ m} \text{ length}$ | `Socket_Shoulder_R` | Carried balanced across one shoulder by laborer. |
| **Dressed Stone Block** | $0.40\text{ m} \times 0.25\text{ m} \times 0.30\text{ m}$ | `Socket_Hands_Front` | Carried with both arms against chest. |
| **Water Pot (*Ghyampo / Amkhora*)**| $0.35\text{ m} \text{ diameter} \times 0.45\text{ m} \text{ height}$ | `Socket_Hip_R` / `Socket_Hands` | Red earthenware water jug. |
| **Felling Axe (*Bancharo*)** | $0.22\text{ m} \times 0.85\text{ m} \times 0.04\text{ m}$ | `Socket_Hand_R` | Forged iron blade on curved hardwood haft. |
| **Mining Pickaxe (*Kodaliko*)** | $0.45\text{ m} \times 0.80\text{ m} \times 0.05\text{ m}$ | `Socket_Hand_R` | Double-ended point for fracturing granite stone. |
| **Wooden Milk Pail / Churn (*Theki*)**| $0.30\text{ m} \text{ diameter} \times 0.50\text{ m} \text{ height}$ | `Socket_Hand_R` | Traditional carved wooden vessel for dairy. |

---

## 3. The Three Construction Stages (Mandatory per Building)

Every building asset must be authored with **three distinct modular sub-models** to support the physical construction lifecycle:

```
[Stage 0: Foundation / Ground Prep]
  └── Cleared level earth pad, wooden perimeter stakes connected with hemp twine,
      small gravel fill, and a tool box with wooden mallet.

[Stage 1: Framing & Scaffolding]
  └── Heavy mortise-and-tenon Sal timber post-and-beam frame erected.
      Bamboo pole scaffolding tied with hemp rope along two sides.
      Partial lower brickwork/wattle walls at waist height (1.0m).

[Stage 2: Completed / Operational Structure]
  └── Fully finished building with weatherproof thatch/terracotta tile roof,
      carved lintels, functioning doors, window grilles, and socketed props.
```

---

## 4. Technical Mesh & LOD Standards

| Asset Category | LOD0 (Close: 5m–15m) | LOD1 (Play: 15m–40m) | LOD2 (Overview: 40m+) | Draw Call Budget |
|---|---|---|---|---|
| **Civic Buildings** | 4,000 – 6,000 tris | 2,000 tris | 800 tris | 1 Submesh / Material |
| **Dwellings / Barns** | 2,500 – 4,000 tris | 1,200 tris | 500 tris | 1 Submesh / Material |
| **Props & Workbenches** | 300 – 800 tris | 150 tris | 50 tris (or cull) | Shared Atlas Material |
| **Human Characters** | 3,500 – 5,000 tris | 1,800 tris | 600 tris | 1 Skinned Mesh |
| **Zebu Cattle** | 3,000 – 4,000 tris | 1,400 tris | 500 tris | 1 Skinned Mesh |
| **Mature Trees** | 2,500 – 4,000 tris | 1,000 tris | 300 tris (Cross-billboard)| 2 Materials (Bark, Foliage) |

---

## 5. PBR Material & Texture Standards (Unity URP)

### Texture Map Structure
- **Workflow:** Metallic / Roughness PBR.
- **`_BaseMap` (sRGB):** Albedo color. Neutral diffuse lighting baked out; natural saturation. Alpha channel = Transparency / Cutout (Foliage only).
- **`_BumpMap` (Linear):** Normal map (OpenGL standard: $+X$ right, $+Y$ up).
- **`_MaskMap` (Linear Channel-Packed):**
  - **Red Channel:** Metallic ($0.0$ for organic stone/timber/thatch; $0.8$–$0.9$ for iron/bronze tools).
  - **Green Channel:** Ambient Occlusion (Cavity and contact shadows).
  - **Blue Channel:** Detail Mask (Optional secondary micro-detail blend).
  - **Alpha Channel:** Smoothness ($1.0 - \text{Roughness}$).

### Authentic Material Values
- **Unfinished Sal Timber:** Albedo `#4A3525`, Metallic `0.0`, Smoothness `0.20`–`0.30`.
- **Himalayan Red Clay / Brick:** Albedo `#A84A2A`, Metallic `0.0`, Smoothness `0.15`–`0.25`.
- **Monsoon River Granite:** Albedo `#62686B`, Metallic `0.0`, Smoothness `0.35` (increases to `0.85` when wet).
- **Dry Thatch / Straw:** Albedo `#B89B5C`, Metallic `0.0`, Smoothness `0.10`–`0.15`.
- **Wrought Iron Tools:** Albedo `#2A2A2A`, Metallic `0.85`, Smoothness `0.40`–`0.55`.

---

## 6. Pre-Import Quality Acceptance Checklist

Before any newly authored 3D asset is integrated into the Unity project repository, it must pass this 8-point checklist:

- [ ] **1. Unit Verification:** Bounding box measured in metres matches the table in Section 2 within $\pm 2\%$.
- [ ] **2. Applied Transforms:** Rotation is $(0,0,0)$, Scale is $(1,1,1)$, Position is $(0,0,0)$ in DCC software.
- [ ] **3. Pivot Alignment:** Pivot point is placed at absolute ground level $(Y=0)$ and centered on footprint.
- [ ] **4. Normal Consistency:** Vertex normals point outward; zero inverted/flipped faces; no non-manifold geometry.
- [ ] **5. Grid Snap:** Outer footprint perimeter snaps cleanly to $2.0\text{ m}$ increments.
- [ ] **6. Entrance Standard:** Main entrance doorway measures at least $1.0\text{ m}$ wide $\times 2.0\text{ m}$ high.
- [ ] **7. Sockets Defined:** Empty helper nodes/locators created for standard sockets (`Socket_Door_Entry`, `Socket_Chimney`, etc.).
- [ ] **8. Material Packing:** Textures channel-packed correctly according to Section 5 with zero wasted uncompressed maps.
