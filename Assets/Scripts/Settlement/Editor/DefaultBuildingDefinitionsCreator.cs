using System.IO;
using UnityEngine;
using UnityEditor;
using Dewabhumi.Economy;

namespace Dewabhumi.Settlement.Editor
{
    public static class DefaultBuildingDefinitionsCreator
    {
        [MenuItem("Dewabhumi/Create Default Building Definitions")]
        public static void CreateDefinitions()
        {
            string folder = "Assets/Art/Buildings/Definitions";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // 1. Simple Thatched Ghar (Dwelling)
            CreateOrUpdateDefinition(
                Path.Combine(folder, "SimpleGhar.asset"),
                BuildingType.SimpleGhar,
                "Simple Ghar",
                "Traditional Himalayan dwelling with whitewashed red clay walls and a steep gabled thatch roof. Houses 4 villagers.",
                "Assets/Art/Buildings/Models/worker-hut.glb",
                width: 2, depth: 3, maxSlope: 15f, labor: 100, beds: 4, storage: 10,
                new ResourceCost(ItemType.Timber, 10),
                new ResourceCost(ItemType.LooseStone, 8),
                new ResourceCost(ItemType.Thatch, 6)
            );

            // 2. Dug Well (Kuwa)
            CreateOrUpdateDefinition(
                Path.Combine(folder, "DugWell.asset"),
                BuildingType.DugWell,
                "Dug Well (Kuwa)",
                "Circular dry-stone masonry well providing fresh, clean spring water for settlers.",
                "Assets/Art/Props/Models/roofed-well.glb",
                width: 1, depth: 1, maxSlope: 15f, labor: 50, beds: 0, storage: 0,
                new ResourceCost(ItemType.LooseStone, 12),
                new ResourceCost(ItemType.Timber, 4)
            );

            // 3. Open Stockpile & Tool Shed
            CreateOrUpdateDefinition(
                Path.Combine(folder, "OpenStockpile.asset"),
                BuildingType.OpenStockpile,
                "Open Stockpile",
                "Designated cleared ground pad and lean-to shelter for storing raw timber logs and granite boulders.",
                "Assets/Art/Buildings/Models/storage-shed.glb",
                width: 2, depth: 2, maxSlope: 12f, labor: 60, beds: 0, storage: 40,
                new ResourceCost(ItemType.Timber, 6),
                new ResourceCost(ItemType.LooseStone, 4)
            );

            // 4. Communal Dry Granary (Bhakari)
            CreateOrUpdateDefinition(
                Path.Combine(folder, "DryGranary.asset"),
                BuildingType.DryGranary,
                "Dry Granary (Bhakari)",
                "Elevated storehouse on anti-vermin stone stilts to protect harvested grains, wild foods, and dried fodder from rain.",
                "Assets/Art/Buildings/Models/granary.glb",
                width: 3, depth: 4, maxSlope: 15f, labor: 120, beds: 0, storage: 200,
                new ResourceCost(ItemType.Timber, 16),
                new ResourceCost(ItemType.LooseStone, 12),
                new ResourceCost(ItemType.Thatch, 10)
            );

            // 5. Traditional Stone House
            CreateOrUpdateDefinition(
                Path.Combine(folder, "StoneHouse.asset"),
                BuildingType.StoneHouse,
                "Stone House",
                "Sturdy two-story Himalayan dry-stone dwelling with slate roofing. Houses 6 villagers.",
                "Assets/Art/Buildings/Models/stone-house.glb",
                width: 3, depth: 3, maxSlope: 15f, labor: 150, beds: 6, storage: 20,
                new ResourceCost(ItemType.LooseStone, 20),
                new ResourceCost(ItemType.Timber, 14),
                new ResourceCost(ItemType.Thatch, 8)
            );

            // 6. Traditional Watermill (Pani Ghatta)
            CreateOrUpdateDefinition(
                Path.Combine(folder, "Watermill.asset"),
                BuildingType.YantraWatermill,
                "Watermill (Pani Ghatta)",
                "Hydraulic wooden grain mill driven by Bagmati river current to grind harvest grains into flour.",
                "Assets/Art/Buildings/Models/watermill.glb",
                width: 2, depth: 2, maxSlope: 18f, labor: 140, beds: 0, storage: 30,
                new ResourceCost(ItemType.Timber, 18),
                new ResourceCost(ItemType.LooseStone, 16)
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DefaultBuildingDefinitionsCreator] Successfully created 6 default BuildingDefinitions in Assets/Art/Buildings/Definitions.");
        }

        private static void CreateOrUpdateDefinition(
            string path, BuildingType type, string displayName, string desc,
            string finishedPrefabPath,
            int width, int depth, float maxSlope, int labor, int beds, int storage,
            params ResourceCost[] costs)
        {
            BuildingDefinition def = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<BuildingDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }

            def.type = type;
            def.displayName = displayName;
            def.description = desc;
            def.widthCells = width;
            def.depthCells = depth;
            def.maxSlopeDegrees = maxSlope;
            def.requiredLaborTicks = labor;
            def.bedCapacity = beds;
            def.storageCapacity = storage;

            if (!string.IsNullOrEmpty(finishedPrefabPath))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(finishedPrefabPath);
                if (prefab != null)
                {
                    def.finishedPrefab = prefab;
                }
            }

            def.requiredMaterials.Clear();
            def.requiredMaterials.AddRange(costs);

            EditorUtility.SetDirty(def);
        }
    }
}
