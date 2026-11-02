using System;
using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Core;
using Dewabhumi.Spatial;

namespace Dewabhumi.Economy
{
    /// <summary>
    /// Central coordinator for all settlement storage facilities and dynamic resource census.
    /// Strictly complies with docs/design/ECONOMY_AND_RESOURCES.md (RES-04).
    /// </summary>
    public class SettlementStorageManager : MonoBehaviour
    {
        public static SettlementStorageManager Instance { get; private set; }

        [Header("Starting Stockpiles")]
        [SerializeField] private bool _spawnStartingStorage = true;
        [SerializeField] private Vector3 _settlementCenter = new Vector3(256f, 12f, 256f);

        private readonly List<StockpileEntity> _stockpiles = new List<StockpileEntity>();
        private EntityRegistry _entityRegistry;
        private Terrain _terrain;

        public IReadOnlyList<StockpileEntity> Stockpiles => _stockpiles;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            _entityRegistry = new EntityRegistry();
        }

        private void Start()
        {
            _terrain = Terrain.activeTerrain;
            if (transform.position.sqrMagnitude > 1f)
            {
                _settlementCenter = transform.position;
            }

            if (_spawnStartingStorage)
            {
                SpawnStartingStorage();
            }
        }

        public void RegisterStockpile(StockpileEntity stockpile)
        {
            if (!_stockpiles.Contains(stockpile))
            {
                _stockpiles.Add(stockpile);
            }
        }

        public void UnregisterStockpile(StockpileEntity stockpile)
        {
            _stockpiles.Remove(stockpile);
        }

        /// <summary>
        /// Computes the total stored physical quantity of an item across all settlement storage.
        /// </summary>
        public int GetTotalStored(ItemType item)
        {
            int sum = 0;
            for (int i = 0; i < _stockpiles.Count; i++)
            {
                if (_stockpiles[i] != null && _stockpiles[i].Inventory != null)
                {
                    sum += _stockpiles[i].Inventory.GetStoredQuantity(item);
                }
            }
            return sum;
        }

        /// <summary>
        /// Computes the total unreserved available quantity across all settlement storage.
        /// </summary>
        public int GetTotalAvailable(ItemType item)
        {
            int sum = 0;
            for (int i = 0; i < _stockpiles.Count; i++)
            {
                if (_stockpiles[i] != null && _stockpiles[i].Inventory != null)
                {
                    sum += _stockpiles[i].Inventory.GetAvailableQuantity(item);
                }
            }
            return sum;
        }

        public StockpileEntity FindNearestStockpileAccepting(Vector3 workerPos, ItemType item, int amount = 1)
        {
            StockpileEntity best = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < _stockpiles.Count; i++)
            {
                var s = _stockpiles[i];
                if (s == null || !s.CanStore(item, amount)) continue;

                float dist = Vector3.Distance(workerPos, s.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    best = s;
                }
            }

            return best;
        }

        public StockpileEntity FindNearestStockpileSupplying(Vector3 workerPos, ItemType item, int amount = 1)
        {
            StockpileEntity best = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < _stockpiles.Count; i++)
            {
                var s = _stockpiles[i];
                if (s == null || s.Inventory == null) continue;
                if (s.Inventory.GetAvailableQuantity(item) < amount) continue;

                float dist = Vector3.Distance(workerPos, s.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    best = s;
                }
            }

            return best;
        }

        public void SpawnStartingStorage()
        {
            // 1. Starting Open Stockpile for timber and stone (8m east of center)
            Vector3 stockpilePos = _settlementCenter + new Vector3(8f, 0f, 0f);
            if (_terrain != null) stockpilePos.y = _terrain.SampleHeight(stockpilePos);

            ulong stockpileId = _entityRegistry.AllocateEntityId(EntityType.Stockpile);
            GameObject stockpileObj = new GameObject($"Stockpile_Open_{stockpileId}");
            stockpileObj.transform.SetParent(transform);

            StockpileEntity openStockpile = stockpileObj.AddComponent<StockpileEntity>();
            openStockpile.Initialize(stockpileId, StorageType.OpenStockpile, 60, stockpilePos);
            openStockpile.Deposit(ItemType.Timber, 24); // Starting timber stock
            openStockpile.Deposit(ItemType.LooseStone, 16); // Starting stone stock

            // 2. Starting Dry Granary for food, grain, and water (10m north of center)
            Vector3 granaryPos = _settlementCenter + new Vector3(0f, 0f, 10f);
            if (_terrain != null) granaryPos.y = _terrain.SampleHeight(granaryPos);

            ulong granaryId = _entityRegistry.AllocateEntityId(EntityType.Stockpile);
            GameObject granaryObj = new GameObject($"Stockpile_Granary_{granaryId}");
            granaryObj.transform.SetParent(transform);

            StockpileEntity granary = granaryObj.AddComponent<StockpileEntity>();
            granary.Initialize(granaryId, StorageType.DryGranary, 200, granaryPos);
            granary.Deposit(ItemType.Grain, 40);
            granary.Deposit(ItemType.WildFood, 25);
            granary.Deposit(ItemType.Thatch, 15);

            Debug.Log($"[SettlementStorageManager] Spawned starting OpenStockpile (24 Timber, 16 Stone) and DryGranary (40 Grain, 25 Food, 15 Thatch).");
        }
    }
}
