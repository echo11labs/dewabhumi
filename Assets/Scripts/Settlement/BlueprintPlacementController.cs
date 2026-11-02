using System;
using UnityEngine;
using Dewabhumi.Spatial;
using Dewabhumi.Core;
using Dewabhumi.Environment;

namespace Dewabhumi.Settlement
{
    /// <summary>
    /// Interactive controller for blueprint placement, validation, rotation, and site commitment.
    /// Strictly complies with docs/design/SETTLEMENT_AND_BUILDINGS.md (BLD-01 & BLD-02).
    /// </summary>
    public class BlueprintPlacementController : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private BuildingDefinition _activeDefinition;
        [SerializeField] private bool _isPlacementActive = false;
        [SerializeField] private int _currentRotationDegrees = 0;

        [Header("Visual Colors")]
        [SerializeField] private Color _validPlacementColor = new Color(0.2f, 0.85f, 0.2f, 0.45f);
        [SerializeField] private Color _invalidPlacementColor = new Color(0.9f, 0.2f, 0.2f, 0.45f);

        private Camera _mainCam;
        private Terrain _terrain;
        private KathmanduTerrainGenerator _terrainGen;
        private OccupancyGrid _occupancyGrid;
        private EntityRegistry _entityRegistry;

        private GameObject _ghostVisualInstance;
        private Renderer[] _ghostRenderers;
        private Material _ghostMaterial;

        public Vector2Int HoveredCell { get; private set; }
        public bool IsPlacementValid { get; private set; }

        public event Action<ConstructionSite> OnBlueprintPlaced;

        private void Awake()
        {
            _mainCam = Camera.main;
            _occupancyGrid = new OccupancyGrid();
            _entityRegistry = new EntityRegistry();

            _isPlacementActive = false;
            _activeDefinition = null;

            CreateGhostMaterial();
        }

        private void Start()
        {
            _terrain = Terrain.activeTerrain;
            if (_terrain != null)
            {
                _terrainGen = _terrain.GetComponent<KathmanduTerrainGenerator>();
            }
        }

        private void Update()
        {
            if (!_isPlacementActive || _activeDefinition == null)
            {
                if (_ghostVisualInstance != null && _ghostVisualInstance.activeSelf)
                {
                    _ghostVisualInstance.SetActive(false);
                }
                return;
            }

            HandleRotationInput();
            UpdateRaycastAndValidation();
            HandleCommitOrCancel();
        }

        public void StartPlacement(BuildingDefinition definition)
        {
            _activeDefinition = definition;
            _isPlacementActive = true;
            _currentRotationDegrees = 0;

            RecreateGhostVisual();
        }

        public void CancelPlacement()
        {
            _isPlacementActive = false;
            _activeDefinition = null;

            if (_ghostVisualInstance != null)
            {
                _ghostVisualInstance.SetActive(false);
            }
        }

        private void HandleRotationInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _currentRotationDegrees = (_currentRotationDegrees + 90) % 360;
                UpdateGhostTransform();
            }
        }

        private void UpdateRaycastAndValidation()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                HoveredCell = SpatialCoordinateService.WorldToGridCell(hit.point);
                Vector2Int footprint = _activeDefinition.GetRotatedFootprint(_currentRotationDegrees);

                IsPlacementValid = ValidatePlacement(HoveredCell, footprint);

                if (_ghostVisualInstance == null)
                {
                    RecreateGhostVisual();
                }

                UpdateGhostTransform();
                UpdateGhostColor(IsPlacementValid ? _validPlacementColor : _invalidPlacementColor);

                if (_ghostVisualInstance != null && !_ghostVisualInstance.activeSelf)
                {
                    _ghostVisualInstance.SetActive(true);
                }
            }
            else
            {
                IsPlacementValid = false;
                if (_ghostVisualInstance != null && _ghostVisualInstance.activeSelf)
                {
                    _ghostVisualInstance.SetActive(false);
                }
            }
        }

        private bool ValidatePlacement(Vector2Int originCell, Vector2Int footprint)
        {
            // 1. Check cell occupancy
            if (!_occupancyGrid.CanOccupyArea(originCell, footprint))
            {
                return false;
            }

            // 2. Terrain slope and water checks
            if (_terrain == null) _terrain = Terrain.activeTerrain;
            if (_terrain != null && _terrain.terrainData != null)
            {
                TerrainData tData = _terrain.terrainData;
                Vector3 tPos = _terrain.transform.position;

                float waterElevation = _terrainGen != null ? _terrainGen.WaterSurfaceElevation : 10f;

                for (int z = 0; z < footprint.y; z++)
                {
                    for (int x = 0; x < footprint.x; x++)
                    {
                        Vector2Int checkCell = new Vector2Int(originCell.x + x, originCell.y + z);
                        Vector3 worldPos = SpatialCoordinateService.GridCellToWorldCenter(checkCell);

                        float normX = (worldPos.x - tPos.x) / tData.size.x;
                        float normZ = (worldPos.z - tPos.z) / tData.size.z;

                        if (normX < 0f || normX > 1f || normZ < 0f || normZ > 1f)
                        {
                            return false; // Out of bounds
                        }

                        // Sample slope
                        float steepness = tData.GetSteepness(normX, normZ);
                        if (steepness > _activeDefinition.maxSlopeDegrees)
                        {
                            return false; // Too steep
                        }

                        // Sample elevation vs. river water level
                        float sampleHeight = _terrain.SampleHeight(worldPos);
                        if (!_activeDefinition.requiresWaterPlacement && sampleHeight < waterElevation + 0.3f)
                        {
                            return false; // Submerged under river
                        }
                    }
                }
            }

            return true;
        }

        private void HandleCommitOrCancel()
        {
            // Left click to commit
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPlacementValid)
                {
                    CommitBlueprint();
                }
            }

            // Right click or Escape to cancel
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }

        private void CommitBlueprint()
        {
            Vector2Int footprint = _activeDefinition.GetRotatedFootprint(_currentRotationDegrees);
            ulong siteId = _entityRegistry.AllocateEntityId(EntityType.ConstructionSite);

            // Register spatial occupancy
            _occupancyGrid.OccupyArea(HoveredCell, footprint, siteId);

            // Spawn ConstructionSite GameObject
            GameObject siteObj = new GameObject($"Site_{_activeDefinition.displayName}_{siteId}");
            Vector3 worldPos = GetPlacementCenterWorld(HoveredCell, footprint);
            siteObj.transform.position = worldPos;
            siteObj.transform.rotation = Quaternion.Euler(0f, _currentRotationDegrees, 0f);

            ConstructionSite site = siteObj.AddComponent<ConstructionSite>();
            site.Initialize(siteId, _activeDefinition, HoveredCell, _currentRotationDegrees);

            OnBlueprintPlaced?.Invoke(site);
            Debug.Log($"[BlueprintPlacementController] Placed {_activeDefinition.displayName} (ID: {siteId}) at cell {HoveredCell}.");

            // Keep placement active if Shift is held, otherwise end placement
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                CancelPlacement();
            }
        }

        private Vector3 GetPlacementCenterWorld(Vector2Int originCell, Vector2Int footprint)
        {
            float centerX = (originCell.x * 2.0f) + (footprint.x * 1.0f);
            float centerZ = (originCell.y * 2.0f) + (footprint.y * 1.0f);

            float y = 0f;
            if (_terrain != null)
            {
                y = _terrain.SampleHeight(new Vector3(centerX, 0f, centerZ));
            }

            return new Vector3(centerX, y, centerZ);
        }

        private void UpdateGhostTransform()
        {
            if (_ghostVisualInstance == null || _activeDefinition == null) return;

            Vector2Int footprint = _activeDefinition.GetRotatedFootprint(_currentRotationDegrees);
            Vector3 center = GetPlacementCenterWorld(HoveredCell, footprint);

            _ghostVisualInstance.transform.position = center;
            _ghostVisualInstance.transform.rotation = Quaternion.Euler(0f, _currentRotationDegrees, 0f);
        }

        private void UpdateGhostColor(Color color)
        {
            if (_ghostMaterial != null)
            {
                _ghostMaterial.color = color;
            }
            if (_ghostRenderers != null)
            {
                for (int i = 0; i < _ghostRenderers.Length; i++)
                {
                    if (_ghostRenderers[i] != null) _ghostRenderers[i].material = _ghostMaterial;
                }
            }
        }

        private void RecreateGhostVisual()
        {
            if (_ghostVisualInstance != null)
            {
                Destroy(_ghostVisualInstance);
            }

            if (_activeDefinition != null && _activeDefinition.finishedPrefab != null)
            {
                _ghostVisualInstance = Instantiate(_activeDefinition.finishedPrefab);
                _ghostVisualInstance.name = "GhostPreview";

                _ghostRenderers = _ghostVisualInstance.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < _ghostRenderers.Length; i++)
                {
                    _ghostRenderers[i].material = _ghostMaterial;
                }

                var colliders = _ghostVisualInstance.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++) Destroy(colliders[i]);
            }
            else
            {
                _ghostVisualInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _ghostVisualInstance.name = "GhostPreview";

                Vector2Int fp = _activeDefinition != null ? _activeDefinition.GetRotatedFootprint(_currentRotationDegrees) : new Vector2Int(2, 2);
                _ghostVisualInstance.transform.localScale = new Vector3(fp.x * 2.0f * 0.98f, 3.2f, fp.y * 2.0f * 0.98f);

                var renderer = _ghostVisualInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = _ghostMaterial;
                    _ghostRenderers = new Renderer[] { renderer };
                }

                var col = _ghostVisualInstance.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }

            _ghostVisualInstance.SetActive(false);
        }

        private void CreateGhostMaterial()
        {
            Shader standardShader = Shader.Find("Universal Render Pipeline/Lit");
            if (standardShader == null) standardShader = Shader.Find("Standard");

            _ghostMaterial = new Material(standardShader);
            _ghostMaterial.color = _validPlacementColor;

            // Set to transparent blend mode
            _ghostMaterial.SetFloat("_Surface", 1); // Transparent
            _ghostMaterial.SetFloat("_Blend", 0);   // Alpha
            _ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _ghostMaterial.SetInt("_ZWrite", 0);
            _ghostMaterial.renderQueue = 3000;
        }
    }
}
