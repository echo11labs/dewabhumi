# DEWABHUMI — Master Documentation Suite

> **Dewabhumi (देबभूमि — 'The Sacred Land / Valley of the Gods')**  
> A historically grounded Himalayan settlement survival simulator set in the Kathmandu Valley.

---

## 1. Project Vision & Identity

**Dewabhumi** is a settlement simulation game that puts the player in the role of the founder and chief of an ancient Himalayan community. Drawing inspiration from the deep topography, hydrology, and cultural ecology of the Kathmandu Valley, players guide their community from an initial pastoral encampment into a thriving, self-sustaining valley settlement.

### Core Pillars
1. **Physical Conservation Invariant:** No phantom resources. Every log, stone, grain basket, and drop of water exists at an authoritative physical coordinate or within an atomic inventory.
2. **Organic Himalayan Settlement:** Step-by-step village growth on authentic valley topography, featuring 2-metre logical grid placement, terrace agriculture (*Yantra*), gravity-fed irrigation channels (*Kulo*), and traditional vernacular architecture.
3. **Pastoral & Agricultural Rhythms:** Deep cattle management (Zebu cattle herds, pasture rotation, fodder harvesting, milk processing) integrated with seasonal monsoon agriculture.
4. **Legible, Farthest Frontier-Inspired Interface:** Clean, desktop-first UI with high-frequency management actions accessible instantly, contextual inspectables, spatial overlays, and informative status feeds.
5. **Deterministic Simulation Architecture:** Fixed-tick simulation decoupled from rendering cadence, enabling robust save/load integrity, replays, and rock-solid state management.

---

## 2. Master Documentation Directory

```
docs/
├── README.md                          # Master documentation index, vision, project structure
├── GDD.md                             # Comprehensive Game Design Document (The Living Core Contract)
│
├── design/                            # Gameplay Systems & Mechanics
│   ├── CORE_LOOP.md                   # Core loop: Gather -> Haul -> Store -> Build -> Assign -> Produce -> Care -> Expand
│   ├── SPATIAL_AND_GRID.md            # Regional Space, 2m Settlement Grid, Coordinate Systems, Scaling & Heights
│   ├── ECONOMY_AND_RESOURCES.md       # Resource Conservation, Stockpiles, Ledgers, Logistics & Delivery
│   ├── CITIZENS_AND_LABOR.md          # Villager AI, Needs, Health, Professions, Tasks & Pathfinding
│   ├── SETTLEMENT_AND_BUILDINGS.md    # Blueprints, Construction Stages, Dwellings, Workplaces & Civic Buildings
│   ├── HERD_AND_CATTLE.md             # Pasture, Fodder, Breeding, Milk Production & Barn Management
│   ├── YANTRA_AND_FARMING.md          # Terrace Farming, Crop Cycles, Seasons, Waterways & Soil Mechanics
│   └── SIMULATION_ARCHITECTURE.md     # Fixed-tick Deterministic Engine, State Registry, Events & Save/Load
│
├── world/                             # World, Lore & Geography
│   ├── KATHMANDU_VALLEY.md            # Valley Geography, Elevation, Bagmati Hydrology & Climate
│   └── HISTORICAL_FOUNDATIONS.md      # Historical inspiration, Licchavi/Newari architecture, cultural authenticity
│
├── art_and_ui/                        # Visuals, Audio & Interface
│   ├── ART_DIRECTION.md               # Visual Pillars, Perspective, Lighting, Palettes & Silhouette Clarity
│   ├── UI_CONTRACT.md                 # Definitive HUD Specification, Top Strip, Build Bar, Inspection & Alerts
│   └── ASSET_PIPELINE.md              # 3D Meshes, Textures, LODs, Scale Rules & Import Standards
│
└── roadmap/                           # Development & Milestones
    ├── ROADMAP.md                     # Phased Development Roadmap (Pre-Alpha MVP -> Early Access -> Release)
    └── BACKLOG.md                     # Prioritized Implementation Backlog & Work Breakdown
```

---

## 3. Technology Stack & Development Standards

- **Engine:** Unity 6 LTS (`6000.0+`), Universal Render Pipeline (URP).
- **Presentation:** 2.5D Isometric / Orthographic perspective with continuous zoom and directional sunlight.
- **Simulation Layer:** Pure C# simulation logic operating on a fixed discrete tick (e.g., 10 Hz), strictly isolated from presentation `GameObject` hierarchies.
- **UI Framework:** Unity UI Toolkit (USS / UXML) for responsive, vectorized HUD elements with crisp typography and styling.
- **Assets:** High-quality modular 3D buildings, naturalistic vegetation, PBR terrain shaders, and handcrafted 2D iconographic representations.

---

## 4. Documentation Governance

- All design decisions must adhere to the **Material Conservation Invariant** (no spontaneous generation of resources without explicit simulated sources).
- Engine implementations must preserve the strict boundary between **Authoritative Simulation State** and **Presentation Views**.
- Cultural and historical elements must respect the **Historical-Fiction Boundary** documented in [`docs/world/HISTORICAL_FOUNDATIONS.md`](world/HISTORICAL_FOUNDATIONS.md).
