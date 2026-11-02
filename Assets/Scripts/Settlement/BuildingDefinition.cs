using System;
using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Economy;

namespace Dewabhumi.Settlement
{
    [System.Serializable]
    public struct ResourceCost
    {
        public ItemType item;
        public int amount;

        public ResourceCost(ItemType item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }

    /// <summary>
    /// ScriptableObject defining the blueprint specifications for a structure in Dewabhumi.
    /// Strictly complies with docs/design/SETTLEMENT_AND_BUILDINGS.md.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuildingDefinition", menuName = "Dewabhumi/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        public BuildingType type;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Footprint (2m Grid Units)")]
        [Tooltip("Width in 2m grid cells")]
        public int widthCells = 2;
        [Tooltip("Depth in 2m grid cells")]
        public int depthCells = 3;
        [Tooltip("Maximum ground slope allowed for placement (degrees)")]
        public float maxSlopeDegrees = 15f;
        [Tooltip("Must be placed straddling an active stream or canal")]
        public bool requiresWaterPlacement = false;

        [Header("Construction Costs")]
        public List<ResourceCost> requiredMaterials = new List<ResourceCost>();
        [Tooltip("Total work ticks required by builders to complete construction")]
        public int requiredLaborTicks = 100;

        [Header("Functionality")]
        public int bedCapacity = 0;
        public int storageCapacity = 0;
        public int workerCapacity = 0;

        [Header("Visual Prefabs")]
        [Tooltip("Stage 0: Foundation stakes and cleared earth")]
        public GameObject foundationPrefab;
        [Tooltip("Stage 1: Timber framing and scaffolding")]
        public GameObject framingPrefab;
        [Tooltip("Stage 2: Completed, operational building")]
        public GameObject finishedPrefab;

        public Vector2Int GetRotatedFootprint(int rotationDegrees)
        {
            int normalized = (rotationDegrees / 90) % 2;
            return normalized == 0 ? new Vector2Int(widthCells, depthCells) : new Vector2Int(depthCells, widthCells);
        }

        public Vector3 GetWorldDimensions(int rotationDegrees)
        {
            Vector2Int fp = GetRotatedFootprint(rotationDegrees);
            return new Vector3(fp.x * 2.0f, 3.5f, fp.y * 2.0f);
        }
    }
}
