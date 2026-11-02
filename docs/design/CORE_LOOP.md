# DEWABHUMI — Core Loop Specification

> **State Transitions, Logistics Flow, and Survival Dynamics**

---

## 1. The Eight-Phase Settlement Cycle

```mermaid
flowchart LR
    G[1. Gather] --> H[2. Haul]
    H --> S[3. Store]
    S --> B[4. Build]
    B --> A[5. Assign]
    A --> P[6. Produce]
    P --> C[7. Care]
    C --> E[8. Expand]
    E --> G
```

Every minute of gameplay in **Dewabhumi** is governed by an interconnected, eight-phase loop:

### Phase 1: Gather
- **Action:** Player designates natural source nodes (mature Sal trees, loose granite river boulders, wild berry thickets, river reed beds).
- **Execution:** Assigned laborers travel to the site, equip tools, and execute work ticks.
- **Conservation Rule:** The source node's internal yield decrements per work tick until exhausted. Spontaneous resources never spawn.

### Phase 2: Haul
- **Action:** Extracted resources are packaged into physical transport units (logs, cut stone, woven baskets of grain/food).
- **Execution:** Haulers pick up items into their personal inventory (up to maximum carry capacity: e.g., 20 kg standard, or 1 heavy log).
- **Conservation Rule:** During transit, the resource belongs exclusively to the carrier's inventory `EntityId`.

### Phase 3: Store
- **Action:** Haulers deposit goods at designated storage facilities (Open Stockpiles for heavy raw materials; Dry Storehouses for perishable food and woven goods).
- **Execution:** Storage slots occupy discrete volume/weight. Items in unroofed stockpiles suffer weather degradation during the monsoon season.
- **Conservation Rule:** Global UI tallies are dynamically queried from storage records; there is no abstract global bank.

### Phase 4: Build
- **Action:** Player selects a structure blueprint from the Build Bar and places it on the 2m settlement grid.
- **Validation:** Terrain slope, collision footprint, entry orientation, and waterway clearance are verified.
- **Material Commitment:** The blueprint registers required materials in the settlement simulation ledger, creating exclusive reservations on stored goods.

### Phase 5: Assign
- **Action:** Players adjust profession allocations via the Professions Window or building-specific worker slots.
- **Dispatch Engine:** Unassigned general laborers automatically take up hauling, general gathering, or builder roles based on priority queues.

### Phase 6: Produce
- **Action:** Operational buildings (Wells, Forager Huts, Cattle Barns, Yantra Watermills) convert labor and inputs into refined products.
- **Cycle:** Inputs must be physically present inside the building's input inventory before work can begin.

### Phase 7: Care
- **Action:** Villagers consume food and fresh water daily; cattle graze pasture or consume stored dry fodder.
- **Health & Shelter:** Dwellings provide warmth, rest, and protection from monsoon dampness. Sickness, starvation, and dehydration cause mortality if unaddressed.

### Phase 8: Expand
- **Action:** A prosperous settlement attracts wandering pastoralists and trade, unlocking higher-tier civic structures, stone spouts (*Dhunge Dhara*), and extensive agricultural terraces (*Yantra*).

---

## 2. Survival Stressors & Hazard Management

| Survival Stressor | Root Cause | Consequence & Recovery Action |
|---|---|---|
| **Starvation** | Zero food in storehouse or dwelling pantries. | Villager health declines by 5% per tick. Emergency action: harvest wild greens or slaughter aging livestock. |
| **Dehydration** | Frozen streams or dry season wells. | Rapid fatigue and work stoppage (-50% speed). Emergency action: dig deeper wells or assign water carriers. |
| **Exposure / Cold** | Insufficient dwellings during winter months. | Hypothermia, sickness, and death during sleep. Build thatched timber homes and maintain firewood stockpile. |
| **Herd Starvation** | Overgrazed pasture pens; no dry season hay. | Cattle lose weight, milk stops, cattle mortality. Rotate pasture enclosures and dry summer grass in advance. |
| **Monsoon Bog** | Heavy rainfall on unpaved dirt trails. | Carrier movement slowed by 60%, stalling construction. Pave high-traffic corridors with beaten earth or river stone. |

---

## 3. Atomic State Transfer Contract

To guarantee that resources are never duplicated or deleted during interruptions:
1. **Reservation Phase:** When a building site requests 10 Timber, 10 units in Stockpile A are marked `ReservedFor(Site_ID)`. Other builders cannot claim them.
2. **Pickup Phase:** When Carrier B arrives at Stockpile A, the 10 Timber units are transferred from Stockpile A to Carrier B in a single atomic transaction.
3. **Cancellation Refund:** If the player cancels the blueprint while materials are in transit:
   - Reserved items still in the stockpile have their reservation lock released immediately.
   - Items currently in the hauler's hands are re-tasked to be carried back to the nearest available storage.
