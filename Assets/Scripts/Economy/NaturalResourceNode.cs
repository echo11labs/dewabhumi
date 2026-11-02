using System;
using UnityEngine;
using Dewabhumi.Spatial;

namespace Dewabhumi.Economy
{
    public enum ResourceNodeType
    {
        SalTimberTree = 0,
        GraniteBoulder = 1,
        WildBerryBush = 2,
        RiverReedPatch = 3
    }

    /// <summary>
    /// Interactive natural resource node in the valley (mature tree, granite boulder, foraged bush).
    /// Strictly complies with docs/design/ECONOMY_AND_RESOURCES.md (RES-01, RES-02).
    /// </summary>
    public class NaturalResourceNode : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ulong _entityId;
        [SerializeField] private ResourceNodeType _nodeType;
        [SerializeField] private ItemType _yieldItem = ItemType.Timber;
        [SerializeField] private int _totalYieldUnits = 6;
        [SerializeField] private int _remainingUnits = 6;
        [SerializeField] private int _laborTicksPerUnit = 12;

        [Header("Harvest State")]
        [SerializeField] private bool _isMarkedForHarvest = true;
        [SerializeField] private int _currentWorkTicks = 0;

        public ulong EntityId => _entityId;
        public ResourceNodeType NodeType => _nodeType;
        public ItemType YieldItem => _yieldItem;
        public int RemainingUnits => _remainingUnits;
        public bool IsMarkedForHarvest => _isMarkedForHarvest;
        public bool IsDepleted => _remainingUnits <= 0;

        public Vector2Int GridCell { get; private set; }

        public event Action<NaturalResourceNode> OnNodeDepleted;
        public event Action<GroundResourceItem> OnUnitHarvested;

        private GameObject _visualModel;
        private Terrain _terrain;

        public void Initialize(ulong entityId, ResourceNodeType nodeType, Vector3 worldPosition)
        {
            _entityId = entityId;
            _nodeType = nodeType;

            switch (nodeType)
            {
                case ResourceNodeType.SalTimberTree:
                    _yieldItem = ItemType.Timber;
                    _totalYieldUnits = 6;
                    _laborTicksPerUnit = 15;
                    break;
                case ResourceNodeType.GraniteBoulder:
                    _yieldItem = ItemType.LooseStone;
                    _totalYieldUnits = 4;
                    _laborTicksPerUnit = 12;
                    break;
                case ResourceNodeType.WildBerryBush:
                    _yieldItem = ItemType.WildFood;
                    _totalYieldUnits = 4;
                    _laborTicksPerUnit = 8;
                    break;
                case ResourceNodeType.RiverReedPatch:
                    _yieldItem = ItemType.Thatch;
                    _totalYieldUnits = 4;
                    _laborTicksPerUnit = 6;
                    break;
            }

            _remainingUnits = _totalYieldUnits;
            _isMarkedForHarvest = true;

            transform.position = worldPosition;
            GridCell = SpatialCoordinateService.WorldToGridCell(worldPosition);

            _terrain = Terrain.activeTerrain;

            CreateVisual();
        }

        public void SetMarkedForHarvest(bool marked)
        {
            _isMarkedForHarvest = marked;
        }

        /// <summary>
        /// Work tick executed by a designated harvester (Woodcutter, Stonemason, Forager).
        /// </summary>
        public bool WorkTick(int ticks = 1)
        {
            if (IsDepleted || !_isMarkedForHarvest) return false;

            _currentWorkTicks += ticks;
            if (_currentWorkTicks >= _laborTicksPerUnit)
            {
                _currentWorkTicks = 0;
                _remainingUnits--;

                SpawnHarvestedUnit();

                if (IsDepleted)
                {
                    HandleDepletion();
                    return true;
                }
            }

            return false;
        }

        private void SpawnHarvestedUnit()
        {
            Vector3 dropPos = transform.position + new Vector3(UnityEngine.Random.Range(-1.2f, 1.2f), 0f, UnityEngine.Random.Range(-1.2f, 1.2f));
            if (_terrain != null)
            {
                dropPos.y = _terrain.SampleHeight(dropPos);
            }

            GameObject itemObj = new GameObject($"Item_{_yieldItem}_{_entityId}_{_remainingUnits}");
            GroundResourceItem groundItem = itemObj.AddComponent<GroundResourceItem>();
            groundItem.Initialize(_entityId * 100 + (ulong)_remainingUnits, _yieldItem, 1, dropPos);

            OnUnitHarvested?.Invoke(groundItem);
            Debug.Log($"[NaturalResourceNode] Harvested 1 {_yieldItem} from {_nodeType} (Remaining: {_remainingUnits})");
        }

        private void HandleDepletion()
        {
            OnNodeDepleted?.Invoke(this);

            if (_nodeType == ResourceNodeType.SalTimberTree)
            {
                // Convert to tree stump visual
                if (_visualModel != null) Destroy(_visualModel);
                CreateStumpVisual();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void CreateVisual()
        {
            _visualModel = new GameObject("Visual_Node");
            _visualModel.transform.SetParent(transform, false);

            switch (_nodeType)
            {
                case ResourceNodeType.SalTimberTree:
                    CreateCrossQuadVisual(_visualModel.transform, "Assets/Art/Environment/Textures/broadleaf-tree-bushy.png", new Vector2(7.5f, 12f), 0f);
                    break;

                case ResourceNodeType.GraniteBoulder:
#if UNITY_EDITOR
                    var stonePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Props/Models/stone-pile.glb");
                    if (stonePrefab != null)
                    {
                        var stoneObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(stonePrefab, _visualModel.transform);
                        stoneObj.transform.localPosition = Vector3.zero;
                        stoneObj.transform.localScale = Vector3.one * 1.2f;
                        break;
                    }
#endif
                    // Fallback primitive
                    GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    boulder.name = "Boulder";
                    boulder.transform.SetParent(_visualModel.transform, false);
                    boulder.transform.localScale = new Vector3(0.95f, 0.65f, 0.80f);
                    boulder.transform.localPosition = new Vector3(0f, 0.32f, 0f);
                    boulder.GetComponent<Renderer>().material.color = new Color(0.50f, 0.54f, 0.56f); // River granite
                    Destroy(boulder.GetComponent<Collider>());
                    break;

                case ResourceNodeType.WildBerryBush:
                    CreateCrossQuadVisual(_visualModel.transform, "Assets/Art/Environment/Textures/small-shrub-fern.png", new Vector2(2.5f, 2.0f), 0f);
                    break;

                case ResourceNodeType.RiverReedPatch:
                    CreateCrossQuadVisual(_visualModel.transform, "Assets/Art/Environment/Textures/wild-grass-tall.png", new Vector2(3.0f, 2.2f), 0f);
                    break;
            }
        }

        private void CreateCrossQuadVisual(Transform parent, string texturePath, Vector2 size, float yOffset)
        {
#if UNITY_EDITOR
            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex != null)
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit == null) urpLit = Shader.Find("Standard");
                Material mat = new Material(urpLit);
                mat.mainTexture = tex;
                mat.SetFloat("_Surface", 0);
                mat.SetFloat("_AlphaClip", 1);
                mat.SetFloat("_Cutoff", 0.30f);
                mat.SetFloat("_Cull", 0); // Double-sided
                mat.EnableKeyword("_ALPHATEST_ON");

                for (int i = 0; i < 3; i++)
                {
                    float angle = i * 60f;
                    GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = $"Quad_{angle}";
                    quad.transform.SetParent(parent, false);
                    quad.transform.localPosition = new Vector3(0f, yOffset + size.y * 0.5f, 0f);
                    quad.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                    quad.transform.localScale = new Vector3(size.x, size.y, 1f);
                    quad.GetComponent<Renderer>().material = mat;
                    Destroy(quad.GetComponent<Collider>());
                }
                return;
            }
#endif
            // Primitive fallback if texture not found
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallback.transform.SetParent(parent, false);
            fallback.transform.localScale = new Vector3(size.x * 0.7f, size.y * 0.7f, size.x * 0.7f);
            fallback.transform.localPosition = new Vector3(0f, size.y * 0.35f, 0f);
            fallback.GetComponent<Renderer>().material.color = new Color(0.28f, 0.48f, 0.22f);
            Destroy(fallback.GetComponent<Collider>());
        }

        private void CreateStumpVisual()
        {
            GameObject stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stump.name = "Visual_TreeStump";
            stump.transform.SetParent(transform, false);
            stump.transform.localScale = new Vector3(0.75f, 0.35f, 0.75f);
            stump.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            stump.GetComponent<Renderer>().material.color = new Color(0.35f, 0.24f, 0.16f);
            Destroy(stump.GetComponent<Collider>());
        }
    }
}
