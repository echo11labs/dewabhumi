# DEWABHUMI — HUD & UI Contract

> **Desktop-First Information Architecture, Farthest Frontier Principles, and Interaction Standards**

---

## 1. Master Layout Grid

The **Dewabhumi** user interface adopts the proven, high-legibility desktop layout principles of *Farthest Frontier*, customized for our Himalayan settlement setting:

```
+----------------------------------------------------------------------------------------------------+
| [Settlement Name: DEWABHUMI]  [Year 1, Spring]  [Pop: 8/12]  [Health: 94%]  |  [Timber: 40] [Stone: 24] [Water: 80] [Food: 110] [Milk: 16] | [Menu / Settings] |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  [!] ALERTS FEED                                                                                   |
|  - Starvation Risk (0)                                                                             |
|  - 2 Workers Idle                                                                                  |
|  - Barn Pasture Full                                                                               |
|                                                                                                    |
|                                                                                                    |
|                                                                          +-----------------------+ |
|                                                                          | CONTEXT INSPECTION    | |
|                                                                          | Selected: Simple Ghar | |
|                                                                          | Condition: 100%       | |
|                                                                          | Occupants: 4/4        | |
|                                                                          | Pantry: 8 Food, 6 H2O | |
|                                                                          +-----------------------+ |
+----------------------------------------------------------------------------------------------------+
| [ || ] [ > ] [ >> ] [ >>> ]   |   [Infrastructure] [Housing] [Production] [Cattle & Farms] [Roads] |
| 1x Speed - Spring Day 4       |   [B] Build Menu   [P] Professions  [L] Ledger   [F3] Diagnostics  |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Core HUD Panels & Hierarchy

### Persistent Top Status Bar
1. **Left:** Settlement crest, Settlement Name, Current Year & Season (with a subtle progress ring indicating seasonal transition).
2. **Center-Left:** Population tally (Adults / Children / Total Capacity), General Health average, Village Happiness.
3. **Center-Right:** High-frequency resource tallies (Timber, Loose Stone, Thatch, Fresh Water, Stored Food, Fresh Milk). Hovering expands a detailed breakdown of stored vs. reserved items.
4. **Right:** System menu, Help/Guide, and audio toggle.

### Bottom Navigation & Build Bar
- **Time Controls (Bottom-Left):**
  - Pause (`Spacebar`), 1x Normal Speed (`1`), 2x Fast Speed (`2`), 4x Maximum Speed (`3`).
  - Current Date, In-Game Time of Day, and Weather Icon (Clear, Monsoon Downpour, Dense Fog, Freezing Frost).
- **Action Categories (Bottom-Center):**
  - **Infrastructure:** Wells, Stockpiles, Storehouses, Chautaras.
  - **Housing:** Simple Thatched Dwelling, Clan Longhouse.
  - **Production:** Woodcutter's Post, Stonemason's Clearing, Forager's Lodge.
  - **Pastoral & Farm:** Cattle Barn, Hay Storage, Terraced Plot, Irrigation Sluice.
  - **Roads & Paths:** Dirt Track, Beaten Earth Road, Paved Stone Walkway.

### Contextual Inspection Card (Floating / Bottom-Right)
Clicking any object in the world displays its dedicated card:
- **Villager Card:** Name, age, profession, current task ("Hauling Timber to Dwelling #3"), physical needs (Hydration, Satiety, Rest bars), inventory payload.
- **Building Card:** Structural condition, assigned workers, inventory contents, production recipe toggle, demolish/repair buttons.
- **Resource Node Card:** Remaining resource yield (e.g., "Sal Tree: 6 Timber remaining"), access status.

### Alert Feed (Left Screen Margin)
Event-driven notification cards with color-coded severity:
- **Red (Urgent):** Citizen starving, shelter collapse, critical water shortage. Clicking focuses the camera on the victim.
- **Yellow (Warning):** Storage full, worker idle with no tasks, building blueprint blocked.
- **Green (Milestone):** New child born, season changed, building construction finished.
