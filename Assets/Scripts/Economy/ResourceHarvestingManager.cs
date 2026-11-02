using System;
using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Core;
using Dewabhumi.Spatial;

namespace Dewabhumi.Economy
{
    /// <summary>
    /// Coordinates valley natural resource nodes, cluster generation, and ground item logistics.
    /// Strictly complies with docs/design/ECONOMY_AND_RESOURCES.md (RES-01 to RES-04).
    /// </summary>
    public class ResourceHarvestingManager : MonoBehaviour
    {
        public static ResourceHarvestingManager Instance { get; private set; }

        [Header("Starting Clusters")]
        [SerializeField] private bool _spawnClustersOnStart = true;
        [SerializeField] private Vector3 _settlementCenter = new Vector3(256f, 12f, 256f);

        private readonly List<NaturalResourceNode> _activeNodes = new List<NaturalResourceNode>();
        private readonly List<GroundResourceItem> _groundItems = new List<GroundResourceItem>();

        private EntityRegistry _entityRegistry;
        private Terrain _terrain;

        public IReadOnlyList<NaturalResourceNode> ActiveNodes => _activeNodes;
        public IReadOnlyList<GroundResourceItem> GroundItems => _groundItems;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            _entityRegistry = new EntityRegistry();
        }

        private void Start()
        {
            _terrain = Terrain.activeTerrain;
            if (_spawnClustersOnStart)
            {
                SpawnInitialValleyResources();
            }
        }

        public void RegisterNode(NaturalResourceNode node)
        {
            if (!_activeNodes.Contains(node))
            {
                _activeNodes.Add(node);
                node.OnNodeDepleted += HandleNodeDepleted;
                node.OnUnitHarvested += HandleUnitHarvested;
            }
        }

        private void HandleNodeDepleted(NaturalResourceNode node)
        {
            _activeNodes.Remove(node);
        }

        private void HandleUnitHarvested(GroundResourceItem item)
        {
            RegisterGroundItem(item);
        }

        public void RegisterGroundItem(GroundResourceItem item)
        {
            if (!_groundItems.Contains(item))
            {
                _groundItems.Add(item);
            }
        }

        public void RemoveGroundItem(GroundResourceItem item)
        {
            _groundItems.Remove(item);
        }

        /// <summary>
        /// Finds the nearest available natural node of a given item type for a worker.
        /// </summary>
        public NaturalResourceNode FindNearestHarvestNode(Vector3 workerPos, ItemType item)
        {
            NaturalResourceNode bestNode = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < _activeNodes.Count; i++)
            {
                var node = _activeNodes[i];
                if (node == null || node.IsDepleted || !node.IsMarkedForHarvest) continue;
                if (node.YieldItem != item) continue;

                float dist = Vector3.Distance(workerPos, node.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Finds the nearest unreserved dropped ground resource item.
        /// </summary>
        public GroundResourceItem FindNearestAvailableGroundItem(Vector3 workerPos, ItemType item = ItemType.None)
        {
            GroundResourceItem bestItem = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < _groundItems.Count; i++)
            {
                var gi = _groundItems[i];
                if (gi == null || gi.IsReserved) continue;
                if (item != ItemType.None && gi.ItemType != item) continue;

                float dist = Vector3.Distance(workerPos, gi.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestItem = gi;
                }
            }

            return bestItem;
        }

        public void SpawnInitialValleyResources()
        {
            // 1. Sal Timber Forest Grove (North slope of valley bowl)
            SpawnCluster(ResourceNodeType.SalTimberTree, 12, new Vector3(256f, 0f, 320f), radius: 40f);

            // 2. Granite Boulder Cluster (Riverbanks & shingle of Bagmati River)
            SpawnCluster(ResourceNodeType.GraniteBoulder, 8, new Vector3(145f, 0f, 230f), radius: 22f);

            // 3. Wild Mountain Berry Bushes (Meadow borders of the Plateau)
            SpawnCluster(ResourceNodeType.WildBerryBush, 6, new Vector3(275f, 0f, 245f), radius: 18f);

            // 4. River Reed Beds (Near the water's edge)
            SpawnCluster(ResourceNodeType.RiverReedPatch, 5, new Vector3(135f, 0f, 205f), radius: 15f);

            Debug.Log($"[ResourceHarvestingManager] Spawned {_activeNodes.Count} initial natural resource nodes in Kathmandu Valley.");
        }

        private void SpawnCluster(ResourceNodeType nodeType, int count, Vector3 center, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 circle = UnityEngine.Random.insideUnitCircle * radius;
                Vector3 pos = new Vector3(center.x + circle.x, 0f, center.z + circle.y);

                if (_terrain != null)
                {
                    pos.y = _terrain.SampleHeight(pos);
                }

                ulong id = _entityRegistry.AllocateEntityId(EntityType.ResourceNode);
                GameObject nodeObj = new GameObject($"Node_{nodeType}_{id}");
                nodeObj.transform.SetParent(transform);

                NaturalResourceNode node = nodeObj.AddComponent<NaturalResourceNode>();
                node.Initialize(id, nodeType, pos);
                RegisterNode(node);
            }
        }
    }
}
