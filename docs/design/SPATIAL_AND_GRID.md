# DEWABHUMI — Spatial & Grid Standard

> **Coordinate Systems, Scale Hierarchy, Metric Physical Conversions, and Placement Rules**

---

## 1. The Three Coordinate Spaces

```
+-----------------------------------------------------------------------------------+
| 1. REGIONAL GEOGRAPHIC SPACE (Copernicus GLO-30 Elevation)                        |
| 40 x 40 km square centered near 27.695°N, 85.375°E (UTM Zone 45N / EPSG:32645).   |
| Scale: 1 Unity World Unit = 100 Metres (Regional Context).                        |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| 2. REGIONAL INSPECTION & PLANNING GRID                                            |
| 48 x 48 cells centered on the settlement camp. Cell size = 20 Metres (0.2 units). |
| Total span: 960 x 960 Metres. Used for regional scouting and water flow analysis. |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| 3. AUTHORITATIVE SETTLEMENT LOGICAL GRID                                          |
| Discrete integer grid of 2.0 x 2.0 Metre cells (0.02 Regional World Units).       |
| 1 Unity World Unit = 1.0 Metre in Settlement Scene.                               |
| Building footprints, road tiles, occupancy masks, pathfinding, and clearances.    |
+-----------------------------------------------------------------------------------+
```

### Coordinate Conversion Service
All conversions between world space and logical grid coordinates must pass through a single centralized conversion service (`SpatialCoordinateService`):

$$\text{cell}_x = \lfloor \frac{x - \text{origin}_x}{\text{cell\_size}} \rfloor, \quad \text{cell}_z = \lfloor \frac{z - \text{origin}_z}{\text{cell\_size}} \rfloor$$

- **Settlement Grid Tile Size:** Exactly $2.0\text{ m} \times 2.0\text{ m}$.
- **Rotation Constraints:** Building placement is constrained strictly to 90-degree orthogonal increments ($0^\circ, 90^\circ, 180^\circ, 270^\circ$).
- **Deterministic Storage:** Save files record discrete integer grid cell coordinates $(X, Z)$, never floating-point transform positions.

---

## 2. Metric Physical Sizing & Proportion Standards

All 3D models and interactive objects must be authored to exact physical metric scales:

| Entity / Object | Physical Metric Dimension | Unity Settlement Space (1m = 1 Unit) | Logical Grid Cells | Notes |
|---|---|---|---|---|
| **Adult Human Settler** | $1.75\text{ m}$ height, $0.50\text{ m}$ width | $0.50 \times 1.75 \times 0.35$ | $0.25$ cell footprint | Carrying basket sits on back. |
| **Zebu Cow / Bull** | $1.40\text{ m}$ – $1.55\text{ m}$ height | $0.80 \times 1.50 \times 2.20$ | $0.5 \times 1.1$ cells | Distinctive hump and dewlap. |
| **Mature Timber Tree (Sal)** | $16.0\text{ m}$ height, $8.0\text{ m}$ crown | $8.0 \times 16.0 \times 8.0$ | $1$ trunk cell, 4 canopy cells | Felled into 6 haulable logs. |
| **Loose River Boulder** | $0.60\text{ m}$ height, $0.90\text{ m}$ width | $0.90 \times 0.60 \times 0.75$ | $1 \times 1$ cell picking bounds | Yields 4 loose stone units. |
| **Simple Thatched Dwelling**| $4.0\text{ m} \times 6.0\text{ m}$, $3.8\text{ m}$ height | $4.0 \times 3.8 \times 6.0$ | **$2 \times 3$ grid cells** | Houses 4 villagers. |
| **Upgraded Stone House** | $6.0\text{ m} \times 6.0\text{ m}$, $5.5\text{ m}$ height | $6.0 \times 5.5 \times 6.0$ | **$3 \times 3$ grid cells** | Houses 8 villagers. |
| **Open Stockpile & Shed** | $4.0\text{ m} \times 4.0\text{ m}$, $2.8\text{ m}$ height | $4.0 \times 2.8 \times 4.0$ | **$2 \times 2$ grid cells** | Holds 40 raw logs/stone. |
| **Communal Dry Granary** | $6.0\text{ m} \times 8.0\text{ m}$, $4.5\text{ m}$ height | $6.0 \times 4.5 \times 8.0$ | **$3 \times 4$ grid cells** | Holds 200 food/fodder units. |
| **Cattle Barn (*Goth*)** | $8.0\text{ m} \times 8.0\text{ m}$, $4.2\text{ m}$ height | $8.0 \times 4.2 \times 8.0$ | **$4 \times 4$ grid cells** | Holds 4 cattle, 2 herders. |
| **Yantra Watermill** | $6.0\text{ m} \times 6.0\text{ m}$, $3.6\text{ m}$ height | $6.0 \times 3.6 \times 6.0$ | **$3 \times 3$ grid cells** | Must straddle stream/canal. |
| **Dug Well (*Kuwa*)** | $2.0\text{ m} \times 2.0\text{ m}$, $2.4\text{ m}$ height | $2.0 \times 2.4 \times 2.0$ | **$1 \times 1$ grid cell** | Clean water access point. |
| **Dirt Trail / Stone Road** | $2.0\text{ m}$ width | $2.0 \times 0.05 \times 2.0$ | **$1 \times 1$ grid cell** | +20% / +40% speed modifier. |
| **Stream Bridge Module** | $2.0\text{ m} \times 4.0\text{ m}$, $1.2\text{ m}$ height | $2.0 \times 1.2 \times 4.0$ | **$1 \times 2$ grid cells** | Spans water crossings. |

---

## 3. Placement Validation Rules

When the player places a blueprint on the settlement grid, the placement engine validates:
1. **Occupancy Clearance:** All footprint cells must be free of existing structures, boulders, mature trees, or deep water.
2. **Slope Threshold:**
   - **Dwellings & Workshops:** Maximum ground slope $\le 15^\circ$. Exceeding this requires terrace excavation.
   - **Paths & Trails:** Maximum ground slope $\le 25^\circ$. Steeper trails require stone steps.
3. **Access & Entrance:** At least one designated door cell must open onto a walkable, unobstructed cell.
4. **Water & Flood Buffer:** Habitable buildings must sit at least 1 cell ($2\text{ m}$) away from permanent river high-water marks.
