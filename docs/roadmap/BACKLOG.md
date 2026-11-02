# DEWABHUMI — Implementation Backlog

> **Granular Feature Breakdown, Ticket Dependencies, and Priority Tiers**

---

## Priority Tiers
- **P0 (Critical Path):** Required for the playable MVP survival loop.
- **P1 (High Priority):** Core gameplay polish, ergonomics, and feedback.
- **P2 (Medium Priority):** Phase 2 agricultural and seasonal expansions.

---

## 1. Simulation Engine & Foundation

| Ticket ID | Title | Priority | Dependencies | Status |
|---|---|---|---|---|
| **SIM-01** | Pure C# Fixed-Tick Clock (10Hz) & FIFO Command Dispatch | P0 | None | Ready |
| **SIM-02** | Entity Registry & Stable 64-bit ID Allocation | P0 | SIM-01 | Ready |
| **SIM-03** | Spatial Coordinate Service (World <-> 2m Grid conversions) | P0 | None | Ready |
| **SIM-04** | Inventory Component & Atomic Transfer Transactions | P0 | SIM-02 | Ready |
| **SIM-05** | Complete Save/Load Serialization & Interruption Safety | P0 | SIM-02, SIM-04 | Ready |

---

## 2. Resource Logistics & Gathering

| Ticket ID | Title | Priority | Dependencies | Status |
|---|---|---|---|---|
| **RES-01** | Timber Harvesting: Sal Tree felling, log bucking, and yield decrement | P0 | SIM-04 | Ready |
| **RES-02** | River Stone Gathering: Boulder quarrying and pickup | P0 | SIM-04 | Ready |
| **RES-03** | Hauler AI: Dokoko basket inventory, capacity limits, and stockpile deposit | P0 | SIM-04 | Ready |
| **RES-04** | Stockpile Management: Open stockpile (timber/stone) and dry storehouse | P0 | SIM-04 | Ready |
| **RES-05** | Perishability & Spoilage System (Fresh milk, wild berries) | P1 | RES-04 | Planned |

---

## 3. Construction & Settlement

| Ticket ID | Title | Priority | Dependencies | Status |
|---|---|---|---|---|
| **BLD-01** | Blueprint Placement Engine: 2m grid snap & 90° rotation | P0 | SIM-03 | Ready |
| **BLD-02** | Placement Validation: Slope (<15°), occupancy, water buffers, entry clearance | P0 | BLD-01 | Ready |
| **BLD-03** | Construction Pipeline: Reservation -> Hauler delivery -> Builder labor ticks | P0 | BLD-02, RES-03 | Ready |
| **BLD-04** | Dwelling Lifecycle: Bed assignment, resting, and household pantry | P0 | BLD-03 | Ready |
| **BLD-05** | Path Building: Dirt trail & stone road speed modifiers | P0 | SIM-03 | Ready |

---

## 4. Citizens & Pastoral Husbandry

| Ticket ID | Title | Priority | Dependencies | Status |
|---|---|---|---|---|
| **CIT-01** | Villager Physiology: Satiety, Hydration, Rest, and Health state machine | P0 | SIM-02 | Ready |
| **CIT-02** | Autonomous Dispatch Engine: Task priority scoring and job allocation | P0 | CIT-01, RES-03 | Ready |
| **HERD-01**| Zebu Cattle Entity: Pasture pen designation, grazing biomass depletion | P0 | SIM-02 | Ready |
| **HERD-02**| Fodder Management: Green grass grazing vs. winter hay feeding | P0 | HERD-01 | Ready |
| **HERD-03**| Daily Milking & Processing: Milkmaid role, butter/curd churning | P0 | HERD-01, SIM-04 | Ready |

---

## 5. User Interface & Presentation

| Ticket ID | Title | Priority | Dependencies | Status |
|---|---|---|---|---|
| **UI-01**  | Persistent Top Status Bar: Population, health, and resource tallies | P0 | SIM-04, CIT-01 | Ready |
| **UI-02**  | Bottom Build Bar & Category Tabs (Infrastructure, Housing, Production) | P0 | BLD-01 | Ready |
| **UI-03**  | Time Controls: Pause, 1x, 2x, 4x simulation speed | P0 | SIM-01 | Ready |
| **UI-04**  | Contextual Selection Cards: Villager, Building, and Node inspectors | P0 | SIM-02 | Ready |
| **UI-05**  | Event Alert Feed: Starvation warnings, idle workers, resource shortages | P1 | CIT-01, RES-04 | Ready |
