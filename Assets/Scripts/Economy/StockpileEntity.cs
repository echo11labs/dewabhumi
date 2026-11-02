using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Spatial;

namespace Dewabhumi.Economy
{
    public enum StorageType
    {
        OpenStockpile = 0,
        DryGranary = 1
    }

    /// <summary>
    /// Component managing a designated settlement storage facility (Open Stockpile or Dry Granary).
    /// Strictly complies with docs/design/ECONOMY_AND_RESOURCES.md (RES-04).
    /// </summary>
    public class StockpileEntity : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ulong _entityId;
        [SerializeField] private StorageType _storageType = StorageType.OpenStockpile;
        [SerializeField] private int _capacityUnits = 60;

        public ulong EntityId => _entityId;
        public StorageType StorageType => _storageType;
        public InventoryComponent Inventory { get; private set; }
        public Vector2Int GridCell { get; private set; }

        private GameObject _visualInstance;

        public void Initialize(ulong entityId, StorageType storageType, int capacity, Vector3 worldPosition)
        {
            _entityId = entityId;
            _storageType = storageType;
            _capacityUnits = capacity;

            Inventory = new InventoryComponent(entityId, capacity);

            transform.position = worldPosition;
            GridCell = SpatialCoordinateService.WorldToGridCell(worldPosition);

            CreateVisual();

            // Register with storage manager
            if (SettlementStorageManager.Instance != null)
            {
                SettlementStorageManager.Instance.RegisterStockpile(this);
            }
        }

        private void OnDestroy()
        {
            if (SettlementStorageManager.Instance != null)
            {
                SettlementStorageManager.Instance.UnregisterStockpile(this);
            }
        }

        public bool AcceptsItem(ItemType item)
        {
            if (_storageType == StorageType.OpenStockpile)
            {
                // Raw building materials
                return item == ItemType.Timber || item == ItemType.LooseStone || item == ItemType.Clay || item == ItemType.DressedPlank || item == ItemType.StoneBlock;
            }
            else // DryGranary
            {
                // Food, grains, thatch, dried fodder, dairy
                return item == ItemType.Grain || item == ItemType.WildFood || item == ItemType.Thatch || item == ItemType.FreshMilk || item == ItemType.ButterGhee || item == ItemType.DryHay;
            }
        }

        public bool CanStore(ItemType item, int amount)
        {
            if (!AcceptsItem(item)) return false;
            return Inventory.CanAccept(item, amount);
        }

        public bool Deposit(ItemType item, int amount)
        {
            if (!CanStore(item, amount)) return false;
            return Inventory.DepositDirect(item, amount);
        }

        public bool Withdraw(ItemType item, int amount)
        {
            return Inventory.WithdrawDirect(item, amount);
        }

        private void CreateVisual()
        {
            _visualInstance = new GameObject("Visual_Storage");
            _visualInstance.transform.SetParent(transform, false);

#if UNITY_EDITOR
            if (_storageType == StorageType.OpenStockpile)
            {
                var shedPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Buildings/Models/storage-shed.glb");
                if (shedPrefab != null)
                {
                    var shed = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(shedPrefab, _visualInstance.transform);
                    shed.transform.localPosition = Vector3.zero;
                    shed.transform.localScale = Vector3.one * 1.0f;

                    // Place a log pile inside
                    var logPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Props/Models/log-pile.glb");
                    if (logPrefab != null)
                    {
                        var log = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(logPrefab, _visualInstance.transform);
                        log.transform.localPosition = new Vector3(1.2f, 0f, 0f);
                        log.transform.localScale = Vector3.one * 0.8f;
                    }
                    return;
                }
            }
            else // DryGranary
            {
                var granaryPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Buildings/Models/granary.glb");
                if (granaryPrefab != null)
                {
                    var granary = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(granaryPrefab, _visualInstance.transform);
                    granary.transform.localPosition = Vector3.zero;
                    granary.transform.localScale = Vector3.one * 1.0f;
                    return;
                }
            }
#endif

            if (_storageType == StorageType.OpenStockpile)
            {
                // 4.0m x 4.0m open ground pad with boundary timber logs
                GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pad.name = "GroundPad";
                pad.transform.SetParent(_visualInstance.transform, false);
                pad.transform.localScale = new Vector3(4.0f, 0.15f, 4.0f);
                pad.transform.localPosition = new Vector3(0f, 0.075f, 0f);
                pad.GetComponent<Renderer>().material.color = new Color(0.42f, 0.32f, 0.22f); // Packed earth

                // Boundary logs
                CreateLogBoundary(_visualInstance.transform);
                Destroy(pad.GetComponent<Collider>());
            }
            else // DryGranary
            {
                // 6.0m x 8.0m elevated storehouse on stilts
                GameObject stilts = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stilts.name = "Stilts";
                stilts.transform.SetParent(_visualInstance.transform, false);
                stilts.transform.localScale = new Vector3(5.8f, 0.60f, 7.8f);
                stilts.transform.localPosition = new Vector3(0f, 0.30f, 0f);
                stilts.GetComponent<Renderer>().material.color = new Color(0.45f, 0.45f, 0.45f); // Stone stilts

                GameObject granaryBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                granaryBody.name = "GranaryBody";
                granaryBody.transform.SetParent(_visualInstance.transform, false);
                granaryBody.transform.localScale = new Vector3(5.6f, 3.2f, 7.6f);
                granaryBody.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                granaryBody.GetComponent<Renderer>().material.color = new Color(0.65f, 0.48f, 0.32f); // Himalayan timber/mud

                Destroy(stilts.GetComponent<Collider>());
                Destroy(granaryBody.GetComponent<Collider>());
            }
        }

        private void CreateLogBoundary(Transform parent)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = $"Log_{i}";
                log.transform.SetParent(parent, false);
                log.transform.localScale = new Vector3(0.20f, 2.0f, 0.20f);

                float offset = 1.95f;
                switch (i)
                {
                    case 0: // North
                        log.transform.localPosition = new Vector3(0f, 0.20f, offset);
                        log.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                        break;
                    case 1: // South
                        log.transform.localPosition = new Vector3(0f, 0.20f, -offset);
                        log.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                        break;
                    case 2: // East
                        log.transform.localPosition = new Vector3(offset, 0.20f, 0f);
                        log.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                        break;
                    case 3: // West
                        log.transform.localPosition = new Vector3(-offset, 0.20f, 0f);
                        log.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                        break;
                }
                log.GetComponent<Renderer>().material.color = new Color(0.35f, 0.22f, 0.12f);
                Destroy(log.GetComponent<Collider>());
            }
        }
    }
}
