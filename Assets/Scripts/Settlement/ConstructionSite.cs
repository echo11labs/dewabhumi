using System;
using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Economy;

namespace Dewabhumi.Settlement
{
    public enum ConstructionStage
    {
        FoundationStakes = 0,
        FramingAndScaffolding = 1,
        Complete = 2
    }

    /// <summary>
    /// Component managing an active building construction site on the 2m settlement grid.
    /// Governs material delivery, builder labor ticks, and visual stage switching.
    /// Strictly complies with docs/design/SETTLEMENT_AND_BUILDINGS.md (BLD-03).
    /// </summary>
    public class ConstructionSite : MonoBehaviour
    {
        public ulong EntityId { get; private set; }
        public BuildingDefinition Definition { get; private set; }
        public Vector2Int OriginCell { get; private set; }
        public int RotationDegrees { get; private set; }

        public ConstructionStage CurrentStage { get; private set; } = ConstructionStage.FoundationStakes;

        private readonly Dictionary<ItemType, int> _delivered = new Dictionary<ItemType, int>();
        private int _currentLaborTicks = 0;

        public event Action<ConstructionSite> OnConstructionCompleted;
        public event Action<ConstructionStage> OnStageChanged;

        private GameObject _activeStageInstance;

        public void Initialize(ulong entityId, BuildingDefinition definition, Vector2Int originCell, int rotationDegrees)
        {
            EntityId = entityId;
            Definition = definition;
            OriginCell = originCell;
            RotationDegrees = rotationDegrees;

            UpdateVisualStage(ConstructionStage.FoundationStakes);
        }

        public bool IsFullyDelivered()
        {
            foreach (var req in Definition.requiredMaterials)
            {
                int delivered = _delivered.TryGetValue(req.item, out int d) ? d : 0;
                if (delivered < req.amount) return false;
            }
            return true;
        }

        public bool NeedsDelivery(ItemType item, out int remainingAmount)
        {
            remainingAmount = 0;
            foreach (var req in Definition.requiredMaterials)
            {
                if (req.item == item)
                {
                    int delivered = _delivered.TryGetValue(item, out int d) ? d : 0;
                    remainingAmount = Mathf.Max(0, req.amount - delivered);
                    return remainingAmount > 0;
                }
            }
            return false;
        }

        public void DeliverMaterial(ItemType item, int amount)
        {
            int current = _delivered.TryGetValue(item, out int d) ? d : 0;
            _delivered[item] = current + amount;

            if (IsFullyDelivered() && CurrentStage == ConstructionStage.FoundationStakes)
            {
                UpdateVisualStage(ConstructionStage.FramingAndScaffolding);
            }
        }

        public void PerformLaborTick(int ticks = 1)
        {
            if (!IsFullyDelivered()) return;

            _currentLaborTicks += ticks;

            if (_currentLaborTicks >= Definition.requiredLaborTicks)
            {
                CompleteBuilding();
            }
        }

        public float GetLaborProgress01()
        {
            if (Definition.requiredLaborTicks <= 0) return 1f;
            return Mathf.Clamp01((float)_currentLaborTicks / Definition.requiredLaborTicks);
        }

        private void CompleteBuilding()
        {
            CurrentStage = ConstructionStage.Complete;
            UpdateVisualStage(ConstructionStage.Complete);

            // Register functional building component
            if (Definition.type == BuildingType.SimpleGhar || Definition.type == BuildingType.StoneHouse)
            {
                var dwelling = gameObject.AddComponent<DwellingEntity>();
                dwelling.Initialize(EntityId, 4, transform.position);
                Dewabhumi.Citizens.SettlementPopulationManager.Instance?.RegisterDwelling(dwelling);
            }
            else if (Definition.type == BuildingType.OpenStockpile)
            {
                var stockpile = gameObject.AddComponent<StockpileEntity>();
                stockpile.Initialize(EntityId, StorageType.OpenStockpile, 60, transform.position);
                Dewabhumi.Economy.SettlementStorageManager.Instance?.RegisterStockpile(stockpile);
            }
            else if (Definition.type == BuildingType.DryGranary)
            {
                var granary = gameObject.AddComponent<StockpileEntity>();
                granary.Initialize(EntityId, StorageType.DryGranary, 200, transform.position);
                Dewabhumi.Economy.SettlementStorageManager.Instance?.RegisterStockpile(granary);
            }

            OnConstructionCompleted?.Invoke(this);
            Debug.Log($"[ConstructionSite] {Definition.displayName} completed at cell {OriginCell}!");
        }

        private void UpdateVisualStage(ConstructionStage newStage)
        {
            CurrentStage = newStage;
            if (_activeStageInstance != null)
            {
                Destroy(_activeStageInstance);
            }

            GameObject prefabToSpawn = null;
            switch (newStage)
            {
                case ConstructionStage.FoundationStakes:
                    prefabToSpawn = Definition.foundationPrefab;
                    break;
                case ConstructionStage.FramingAndScaffolding:
                    prefabToSpawn = Definition.framingPrefab;
                    break;
                case ConstructionStage.Complete:
                    prefabToSpawn = Definition.finishedPrefab;
                    break;
            }

            if (prefabToSpawn != null)
            {
                _activeStageInstance = Instantiate(prefabToSpawn, transform);
                _activeStageInstance.transform.localPosition = Vector3.zero;
                _activeStageInstance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // Fallback procedural placeholder visual if prefab is not yet assigned
                CreateProceduralStageVisual(newStage);
            }

            OnStageChanged?.Invoke(newStage);
        }

        private void CreateProceduralStageVisual(ConstructionStage stage)
        {
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = $"Visual_{stage}";
            placeholder.transform.SetParent(transform, false);

            Vector2Int fp = Definition.GetRotatedFootprint(RotationDegrees);
            float width = fp.x * 2.0f;
            float depth = fp.y * 2.0f;

            float height = stage switch
            {
                ConstructionStage.FoundationStakes => 0.25f,
                ConstructionStage.FramingAndScaffolding => 1.8f,
                ConstructionStage.Complete => 3.5f,
                _ => 1.0f
            };

            placeholder.transform.localScale = new Vector3(width * 0.95f, height, depth * 0.95f);
            placeholder.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);

            var renderer = placeholder.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color c = stage switch
                {
                    ConstructionStage.FoundationStakes => new Color(0.6f, 0.4f, 0.2f, 0.8f),
                    ConstructionStage.FramingAndScaffolding => new Color(0.8f, 0.6f, 0.2f, 0.9f),
                    ConstructionStage.Complete => new Color(0.3f, 0.6f, 0.3f, 1.0f),
                    _ => Color.white
                };
                renderer.material.color = c;
            }

            // Remove box collider from placeholder so it doesn't block rays
            var col = placeholder.GetComponent<Collider>();
            if (col != null) Destroy(col);

            _activeStageInstance = placeholder;
        }
    }
}
