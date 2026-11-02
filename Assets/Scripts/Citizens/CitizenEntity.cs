using System;
using UnityEngine;
using Dewabhumi.Economy;
using Dewabhumi.Spatial;
using Dewabhumi.Settlement;

namespace Dewabhumi.Citizens
{
    public enum CitizenActionState
    {
        Idle = 0,
        MovingToTarget = 1,
        HarvestingResource = 2,
        DeliveringToSite = 3,
        BuildingStructure = 4,
        SeekingWater = 5,
        SeekingFood = 6,
        SleepingInDwelling = 7
    }

    /// <summary>
    /// Component representing an individual human settler in Dewabhumi.
    /// Executes autonomous harvesting, hauling, building, and physiological survival cycles.
    /// Strictly complies with docs/design/CITIZENS_AND_LABOR.md and docs/art_and_ui/ASSET_PIPELINE.md.
    /// </summary>
    public class CitizenEntity : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private ulong _entityId;
        [SerializeField] private string _citizenName = "Settler";
        [SerializeField] private ProfessionType _profession = ProfessionType.GeneralLaborer;

        [Header("State")]
        [SerializeField] private CitizenActionState _actionState = CitizenActionState.Idle;
        [SerializeField] private float _moveSpeed = 1.4f; // 1.4 m/s standard walking speed

        public ulong EntityId => _entityId;
        public string CitizenName => _citizenName;
        public ProfessionType Profession => _profession;
        public CitizenActionState ActionState => _actionState;
        public ulong HomeDwellingId { get; private set; } = 0;

        public PhysiologyComponent Physiology { get; private set; }
        public InventoryComponent Inventory { get; private set; }
        public Vector2Int GridCell { get; private set; }

        private Vector3 _targetDestination;
        private bool _hasDestination = false;
        private Terrain _terrain;

        // Current target references
        private NaturalResourceNode _targetHarvestNode;
        private GroundResourceItem _targetPickupItem;
        private ConstructionSite _targetConstructionSite;

        public void Initialize(ulong entityId, string name, ProfessionType profession, Vector3 startWorldPos)
        {
            _entityId = entityId;
            _citizenName = name;
            _profession = profession;

            Physiology = new PhysiologyComponent();
            Inventory = new InventoryComponent(entityId, 20); // 20 units basket capacity

            transform.position = startWorldPos;
            GridCell = SpatialCoordinateService.WorldToGridCell(startWorldPos);
            _targetDestination = startWorldPos;

            _terrain = Terrain.activeTerrain;

            CreatePlaceholderVisual();
        }

        private void Update()
        {
            if (_hasDestination)
            {
                MoveTowardsDestination();
            }

            // Sync grid cell coordinate
            GridCell = SpatialCoordinateService.WorldToGridCell(transform.position);
        }

        public void AssignDwelling(ulong dwellingId)
        {
            HomeDwellingId = dwellingId;
        }

        /// <summary>
        /// Called on each 10 Hz fixed simulation tick.
        /// </summary>
        public void SimulationTick(ulong tickNumber)
        {
            bool isSleeping = _actionState == CitizenActionState.SleepingInDwelling;
            Physiology.Tick(isSleeping);

            if (!Physiology.IsAlive) return;

            var clock = SettlementPopulationManager.Instance != null ? SettlementPopulationManager.Instance.Clock : null;
            bool isNight = clock != null && (clock.HourOfDay >= 20 || clock.HourOfDay < 5);

            // 1. Thirst Interrupt
            if (Physiology.IsThirsty)
            {
                ExecuteDrinkLogic();
                return;
            }

            // 2. Hunger Interrupt
            if (Physiology.IsHungry)
            {
                ExecuteEatLogic();
                return;
            }

            // 3. Night / Rest Cycle (BLD-04)
            if (isNight || Physiology.IsExhausted)
            {
                ExecuteSleepLogic();
                return;
            }

            // Wake up if rested and daytime
            if (_actionState == CitizenActionState.SleepingInDwelling && !isNight && !Physiology.IsExhausted)
            {
                _actionState = CitizenActionState.Idle;
            }

            // 4. Execute profession labor
            ExecuteProfessionLogic();
        }

        private void ExecuteDrinkLogic()
        {
            _actionState = CitizenActionState.SeekingWater;
            // Bagmati river bank nearby
            Vector3 riverBank = new Vector3(240f, 10f, 220f);
            float dist = Vector3.Distance(transform.position, riverBank);

            if (dist <= 3.5f)
            {
                Physiology.Drink(60f);
                _hasDestination = false;
                _actionState = CitizenActionState.Idle;
            }
            else if (!_hasDestination)
            {
                SetDestination(riverBank, CitizenActionState.SeekingWater);
            }
        }

        private void ExecuteEatLogic()
        {
            _actionState = CitizenActionState.SeekingFood;
            var storageMgr = SettlementStorageManager.Instance;
            if (storageMgr == null) return;

            StockpileEntity foodStore = null;
            for (int i = 0; i < storageMgr.Stockpiles.Count; i++)
            {
                var s = storageMgr.Stockpiles[i];
                if (s != null && s.Inventory != null && (s.Inventory.GetStoredQuantity(ItemType.WildFood) > 0 || s.Inventory.GetStoredQuantity(ItemType.Grain) > 0))
                {
                    foodStore = s;
                    break;
                }
            }

            if (foodStore != null)
            {
                float dist = Vector3.Distance(transform.position, foodStore.transform.position);
                if (dist <= 3.0f)
                {
                    ItemType toEat = foodStore.Inventory.GetStoredQuantity(ItemType.WildFood) > 0 ? ItemType.WildFood : ItemType.Grain;
                    if (foodStore.Withdraw(toEat, 1))
                    {
                        Physiology.Eat(50f);
                    }
                    _hasDestination = false;
                    _actionState = CitizenActionState.Idle;
                }
                else if (!_hasDestination)
                {
                    SetDestination(foodStore.transform.position, CitizenActionState.SeekingFood);
                }
            }
        }

        private void ExecuteSleepLogic()
        {
            Vector3 restLocation = new Vector3(256f, 12f, 256f); // Default Chautara campfire

            if (HomeDwellingId != 0 && SettlementPopulationManager.Instance != null)
            {
                var dwelling = SettlementPopulationManager.Instance.GetDwelling(HomeDwellingId);
                if (dwelling != null)
                {
                    restLocation = dwelling.WorldPosition;
                }
            }

            float dist = Vector3.Distance(transform.position, restLocation);
            if (dist <= 2.8f)
            {
                _hasDestination = false;
                _actionState = CitizenActionState.SleepingInDwelling;
            }
            else if (!_hasDestination)
            {
                SetDestination(restLocation, CitizenActionState.MovingToTarget);
            }
        }

        private void ExecuteProfessionLogic()
        {
            switch (_profession)
            {
                case ProfessionType.Woodcutter:
                    ExecuteHarvesterLogic(ItemType.Timber);
                    break;
                case ProfessionType.Stonemason:
                    ExecuteHarvesterLogic(ItemType.LooseStone);
                    break;
                case ProfessionType.Forager:
                    ExecuteHarvesterLogic(ItemType.WildFood);
                    break;
                case ProfessionType.GeneralLaborer:
                    ExecuteHaulerLogic();
                    break;
                case ProfessionType.Builder:
                    ExecuteBuilderLogic();
                    break;
            }
        }

        private void ExecuteHarvesterLogic(ItemType item)
        {
            var mgr = ResourceHarvestingManager.Instance;
            if (mgr == null) return;

            if (_targetHarvestNode == null || _targetHarvestNode.IsDepleted)
            {
                _targetHarvestNode = mgr.FindNearestHarvestNode(transform.position, item);
                if (_targetHarvestNode != null)
                {
                    SetDestination(_targetHarvestNode.transform.position, CitizenActionState.MovingToTarget);
                }
                return;
            }

            float dist = Vector3.Distance(transform.position, _targetHarvestNode.transform.position);
            if (dist <= 2.8f)
            {
                _actionState = CitizenActionState.HarvestingResource;
                // Face the node and work
                Vector3 toNode = (_targetHarvestNode.transform.position - transform.position).normalized;
                toNode.y = 0;
                if (toNode.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(toNode);

                _targetHarvestNode.WorkTick(1);
            }
            else if (!_hasDestination)
            {
                SetDestination(_targetHarvestNode.transform.position, CitizenActionState.MovingToTarget);
            }
        }

        private void ExecuteHaulerLogic()
        {
            var mgr = ResourceHarvestingManager.Instance;
            if (mgr == null) return;

            // 1. If carrying goods in basket, look for an active construction site that needs it
            int totalCarried = Inventory.GetTotalStoredUnits();
            if (totalCarried > 0)
            {
                ConstructionSite site = FindNeedyConstructionSite();
                if (site != null)
                {
                    _targetConstructionSite = site;
                    float dist = Vector3.Distance(transform.position, site.transform.position);
                    if (dist <= 3.5f)
                    {
                        // Deliver all needed carried items
                        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
                        {
                            int stored = Inventory.GetStoredQuantity(item);
                            if (stored > 0 && site.NeedsDelivery(item, out int needed))
                            {
                                int deliverAmount = Mathf.Min(stored, needed);
                                Inventory.WithdrawDirect(item, deliverAmount);
                                site.DeliverMaterial(item, deliverAmount);
                                Debug.Log($"[CitizenEntity] {CitizenName} delivered {deliverAmount} {item} to {site.Definition.displayName}.");
                                break;
                            }
                        }
                        _actionState = CitizenActionState.Idle;
                    }
                    else if (!_hasDestination)
                    {
                        SetDestination(site.transform.position, CitizenActionState.DeliveringToSite);
                    }
                }
                return;
            }

            // 2. Not carrying: find dropped ground resource item to pick up
            if (_targetPickupItem == null || !_targetPickupItem.gameObject.activeInHierarchy)
            {
                _targetPickupItem = mgr.FindNearestAvailableGroundItem(transform.position);
                if (_targetPickupItem != null && _targetPickupItem.TryReserve(EntityId))
                {
                    SetDestination(_targetPickupItem.transform.position, CitizenActionState.MovingToTarget);
                }
                return;
            }

            float distToItem = Vector3.Distance(transform.position, _targetPickupItem.transform.position);
            if (distToItem <= 1.8f)
            {
                // Pick up item
                ItemType type = _targetPickupItem.ItemType;
                int amount = _targetPickupItem.Amount;

                if (Inventory.DepositDirect(type, amount))
                {
                    mgr.RemoveGroundItem(_targetPickupItem);
                    Destroy(_targetPickupItem.gameObject);
                    _targetPickupItem = null;
                    _actionState = CitizenActionState.Idle;
                    Debug.Log($"[CitizenEntity] {CitizenName} loaded {amount} {type} into Dokoko basket.");
                }
            }
            else if (!_hasDestination)
            {
                SetDestination(_targetPickupItem.transform.position, CitizenActionState.MovingToTarget);
            }
        }

        private void ExecuteBuilderLogic()
        {
            ConstructionSite site = FindReadyConstructionSite();
            if (site == null)
            {
                // No ready sites: fallback to hauling materials
                ExecuteHaulerLogic();
                return;
            }

            _targetConstructionSite = site;
            float dist = Vector3.Distance(transform.position, site.transform.position);
            if (dist <= 3.5f)
            {
                _actionState = CitizenActionState.BuildingStructure;
                site.PerformLaborTick(1);
            }
            else if (!_hasDestination)
            {
                SetDestination(site.transform.position, CitizenActionState.MovingToTarget);
            }
        }

        private ConstructionSite FindNeedyConstructionSite()
        {
            ConstructionSite[] sites = FindObjectsByType<ConstructionSite>(FindObjectsSortMode.None);
            for (int i = 0; i < sites.Length; i++)
            {
                if (!sites[i].IsFullyDelivered()) return sites[i];
            }
            return null;
        }

        private ConstructionSite FindReadyConstructionSite()
        {
            ConstructionSite[] sites = FindObjectsByType<ConstructionSite>(FindObjectsSortMode.None);
            for (int i = 0; i < sites.Length; i++)
            {
                if (sites[i].IsFullyDelivered() && sites[i].GetLaborProgress01() < 1f) return sites[i];
            }
            return null;
        }

        public void SetDestination(Vector3 destination, CitizenActionState newState)
        {
            _targetDestination = destination;
            _hasDestination = true;
            _actionState = newState;
        }

        public void StopMovement()
        {
            _hasDestination = false;
            _actionState = CitizenActionState.Idle;
        }

        private void MoveTowardsDestination()
        {
            Vector3 currentPos = transform.position;
            Vector3 target = _targetDestination;

            // Follow ground terrain contour
            if (_terrain != null)
            {
                target.y = _terrain.SampleHeight(target);
                currentPos.y = _terrain.SampleHeight(currentPos);
            }

            Vector3 toTarget = target - currentPos;
            toTarget.y = 0f; // Horizontal movement
            float distance = toTarget.magnitude;

            if (distance <= 0.25f)
            {
                transform.position = target;
                _hasDestination = false;
                if (_actionState == CitizenActionState.MovingToTarget)
                {
                    _actionState = CitizenActionState.Idle;
                }
                return;
            }

            Vector3 step = toTarget.normalized * (_moveSpeed * Time.deltaTime);
            if (step.magnitude > distance) step = toTarget;

            Vector3 newPos = currentPos + step;
            if (_terrain != null)
            {
                newPos.y = _terrain.SampleHeight(newPos);
            }

            transform.position = newPos;

            // Rotate toward direction of movement
            if (toTarget.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget.normalized, Vector3.up), Time.deltaTime * 6f);
            }
        }

        public void ChangeProfession(ProfessionType newProfession)
        {
            _profession = newProfession;
        }

        private void CreatePlaceholderVisual()
        {
            // Human model metric dimensions: 0.5m width, 1.75m height, 0.35m depth
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Visual_Body";
            body.transform.SetParent(transform, false);
            body.transform.localScale = new Vector3(0.5f, 0.875f, 0.35f);
            body.transform.localPosition = new Vector3(0f, 0.875f, 0f);

            var bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = new Color(0.68f, 0.52f, 0.38f); // Himalayan ochre clothing
            }

            // Dokoko Carrying Basket on back
#if UNITY_EDITOR
            var basketPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Props/Models/large-wicker-basket.glb");
            if (basketPrefab != null)
            {
                var basketObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(basketPrefab, transform);
                basketObj.name = "Visual_DokokoBasket";
                basketObj.transform.localPosition = new Vector3(0f, 0.70f, -0.32f);
                basketObj.transform.localScale = Vector3.one * 0.75f;
                basketObj.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
                Destroy(body.GetComponent<Collider>());
                return;
            }
#endif
            GameObject basket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basket.name = "Visual_DokokoBasket";
            basket.transform.SetParent(transform, false);
            basket.transform.localScale = new Vector3(0.45f, 0.65f, 0.40f);
            basket.transform.localPosition = new Vector3(0f, 1.0f, -0.35f);

            var basketRenderer = basket.GetComponent<Renderer>();
            if (basketRenderer != null)
            {
                basketRenderer.material.color = new Color(0.42f, 0.30f, 0.18f); // Woven bamboo brown
            }

            // Remove colliders so they don't interfere with raycasts
            Destroy(body.GetComponent<Collider>());
            Destroy(basket.GetComponent<Collider>());
        }
    }
}
