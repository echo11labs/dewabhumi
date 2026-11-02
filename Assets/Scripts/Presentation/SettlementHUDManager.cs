using System;
using UnityEngine;
using Dewabhumi.Core;
using Dewabhumi.Citizens;
using Dewabhumi.Economy;
using Dewabhumi.Settlement;
using Dewabhumi.Pastoral;

namespace Dewabhumi.Presentation
{
    /// <summary>
    /// Desktop HUD controller implementing the Farthest Frontier layout standard for Dewabhumi.
    /// Strictly complies with docs/art_and_ui/UI_CONTRACT.md (UI-01 to UI-04).
    /// </summary>
    public class SettlementHUDManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SettlementPopulationManager _populationManager;
        [SerializeField] private SettlementStorageManager _storageManager;
        [SerializeField] private BlueprintPlacementController _placementController;

        [Header("Building Definitions")]
        [SerializeField] private BuildingDefinition _simpleGharDef;
        [SerializeField] private BuildingDefinition _dugWellDef;
        [SerializeField] private BuildingDefinition _stockpileDef;
        [SerializeField] private BuildingDefinition _granaryDef;
        [SerializeField] private BuildingDefinition _stoneHouseDef;
        [SerializeField] private BuildingDefinition _watermillDef;

        private GameObject _selectedObject;
        private Camera _mainCam;

        private GUIStyle _topBarStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _statLabelStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _cardHeaderStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _activeButtonStyle;

        private bool _stylesInitialized = false;

        private void Start()
        {
            _mainCam = Camera.main;

            if (_populationManager == null) _populationManager = FindAnyObjectByType<SettlementPopulationManager>();
            if (_storageManager == null) _storageManager = FindAnyObjectByType<SettlementStorageManager>();
            if (_placementController == null) _placementController = FindAnyObjectByType<BlueprintPlacementController>();

            LoadDefaultDefinitionsIfNull();
        }

        private void Update()
        {
            HandleSelectionRaycast();
            HandleHotkeys();
        }

        private void LoadDefaultDefinitionsIfNull()
        {
#if UNITY_EDITOR
            if (_simpleGharDef == null)
                _simpleGharDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Art/Buildings/Definitions/SimpleGhar.asset");
            if (_dugWellDef == null)
                _dugWellDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Art/Buildings/Definitions/DugWell.asset");
            if (_stockpileDef == null)
                _stockpileDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Art/Buildings/Definitions/OpenStockpile.asset");
            if (_granaryDef == null)
                _granaryDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Art/Buildings/Definitions/DryGranary.asset");
            if (_stoneHouseDef == null)
                _stoneHouseDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Art/Buildings/Definitions/StoneHouse.asset");
            if (_watermillDef == null)
                _watermillDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingDefinition>("Assets/Art/Buildings/Definitions/Watermill.asset");
#endif
        }

        private void HandleSelectionRaycast()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Don't select if clicking on GUI or if placement mode is active
                if (_placementController != null && _placementController.IsPlacementValid)
                {
                    return; // Handled by placement controller
                }

                if (_mainCam == null) _mainCam = Camera.main;
                if (_mainCam == null) return;

                Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    // Check if clicked entity
                    var citizen = hit.collider.GetComponentInParent<CitizenEntity>();
                    if (citizen != null)
                    {
                        _selectedObject = citizen.gameObject;
                        return;
                    }

                    var cattle = hit.collider.GetComponentInParent<CattleEntity>();
                    if (cattle != null)
                    {
                        _selectedObject = cattle.gameObject;
                        return;
                    }

                    var site = hit.collider.GetComponentInParent<ConstructionSite>();
                    if (site != null)
                    {
                        _selectedObject = site.gameObject;
                        return;
                    }

                    var node = hit.collider.GetComponentInParent<NaturalResourceNode>();
                    if (node != null)
                    {
                        _selectedObject = node.gameObject;
                        return;
                    }

                    var dwelling = hit.collider.GetComponentInParent<Dewabhumi.Settlement.DwellingEntity>();
                    if (dwelling != null)
                    {
                        _selectedObject = dwelling.gameObject;
                        return;
                    }

                    var storage = hit.collider.GetComponentInParent<StockpileEntity>();
                    if (storage != null)
                    {
                        _selectedObject = storage.gameObject;
                        return;
                    }

                    // Clicked on ground/terrain
                    _selectedObject = null;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _selectedObject = null;
            }
        }

        private void HandleHotkeys()
        {
            // Speed Controls
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKey(KeyCode.LeftControl))
            {
                if (_simpleGharDef != null && _placementController != null)
                    _placementController.StartPlacement(_simpleGharDef);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (_dugWellDef != null && _placementController != null)
                    _placementController.StartPlacement(_dugWellDef);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (_stockpileDef != null && _placementController != null)
                    _placementController.StartPlacement(_stockpileDef);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (_granaryDef != null && _placementController != null)
                    _placementController.StartPlacement(_granaryDef);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                if (_stoneHouseDef != null && _placementController != null)
                    _placementController.StartPlacement(_stoneHouseDef);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                if (_watermillDef != null && _placementController != null)
                    _placementController.StartPlacement(_watermillDef);
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            // Texture background colors
            Texture2D topBarTex = MakeTex(2, 2, new Color(0.12f, 0.14f, 0.16f, 0.92f));
            Texture2D cardTex = MakeTex(2, 2, new Color(0.14f, 0.16f, 0.18f, 0.94f));
            Texture2D buttonTex = MakeTex(2, 2, new Color(0.24f, 0.28f, 0.32f, 0.90f));
            Texture2D activeBtnTex = MakeTex(2, 2, new Color(0.35f, 0.65f, 0.35f, 0.95f));

            _topBarStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = topBarTex },
                padding = new RectOffset(14, 14, 8, 8)
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.45f) } // Himalayan gold
            };

            _statLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.90f, 0.92f, 0.94f) }
            };

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = cardTex },
                padding = new RectOffset(16, 16, 12, 12)
            };

            _cardHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.45f) }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = buttonTex, textColor = Color.white },
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            _activeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = activeBtnTex, textColor = Color.white },
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            DrawTopStatusBar();
            DrawBottomBuildBar();
            DrawInspectionCard();
        }

        private void DrawTopStatusBar()
        {
            float screenW = Screen.width;
            Rect topBarRect = new Rect(10, 10, screenW - 20, 48);
            GUI.Box(topBarRect, GUIContent.none, _topBarStyle);

            GUILayout.BeginArea(new Rect(20, 14, screenW - 40, 40));
            GUILayout.BeginHorizontal();

            // 1. Settlement Title
            GUILayout.Label("DEWABHUMI (देबभूमि)", _titleStyle, GUILayout.Width(220));

            GUILayout.Space(20);

            // 2. Population & Herd
            int settlers = _populationManager != null ? _populationManager.TotalPopulation : 8;
            int cattle = _populationManager != null ? _populationManager.Cattle.Count : 2;
            GUILayout.Label($"Settlers: {settlers}  |  Cattle: {cattle}", _statLabelStyle, GUILayout.Width(170));

            // Date & Season
            if (_populationManager != null && _populationManager.Clock != null)
            {
                var clk = _populationManager.Clock;
                GUILayout.Label($"Yr {clk.CurrentYear} {clk.CurrentSeason} (D{clk.DayOfSeason}) {clk.HourOfDay:D2}:00", _statLabelStyle, GUILayout.Width(160));
            }

            GUILayout.Space(10);

            // 3. Stored Conserved Resources (Timber, Stone, Food, Grain, Thatch)
            int timber = _storageManager != null ? _storageManager.GetTotalStored(ItemType.Timber) : 24;
            int stone = _storageManager != null ? _storageManager.GetTotalStored(ItemType.LooseStone) : 16;
            int food = _storageManager != null ? _storageManager.GetTotalStored(ItemType.WildFood) : 25;
            int grain = _storageManager != null ? _storageManager.GetTotalStored(ItemType.Grain) : 40;
            int thatch = _storageManager != null ? _storageManager.GetTotalStored(ItemType.Thatch) : 15;

            GUILayout.Label($"Timber: {timber}  |  Stone: {stone}  |  Food: {food}  |  Grain: {grain}  |  Thatch: {thatch}", _statLabelStyle);

            GUILayout.FlexibleSpace();

            // 4. Time speed controls
            string speedText = Time.timeScale == 0f ? "PAUSED" : $"{Time.timeScale:0}x";
            if (GUILayout.Button(speedText, _buttonStyle, GUILayout.Width(80), GUILayout.Height(28)))
            {
                Time.timeScale = Time.timeScale == 0f ? 1f : (Time.timeScale == 1f ? 2f : (Time.timeScale == 2f ? 4f : 0f));
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawBottomBuildBar()
        {
            float barWidth = 860f;
            float barHeight = 65f;
            float startX = (Screen.width - barWidth) * 0.5f;
            float startY = Screen.height - barHeight - 15f;

            GUI.Box(new Rect(startX, startY, barWidth, barHeight), GUIContent.none, _cardStyle);

            GUILayout.BeginArea(new Rect(startX + 10, startY + 10, barWidth - 20, barHeight - 20));
            GUILayout.BeginHorizontal();

            // [1] Simple Ghar
            if (GUILayout.Button("[1] Simple Ghar\n10T 8S 6Th", _buttonStyle, GUILayout.Width(135), GUILayout.Height(44)))
            {
                if (_simpleGharDef != null && _placementController != null)
                    _placementController.StartPlacement(_simpleGharDef);
            }

            // [2] Dug Well
            if (GUILayout.Button("[2] Dug Well\n4T 12S", _buttonStyle, GUILayout.Width(135), GUILayout.Height(44)))
            {
                if (_dugWellDef != null && _placementController != null)
                    _placementController.StartPlacement(_dugWellDef);
            }

            // [3] Open Stockpile
            if (GUILayout.Button("[3] Stockpile\n6T 4S", _buttonStyle, GUILayout.Width(135), GUILayout.Height(44)))
            {
                if (_stockpileDef != null && _placementController != null)
                    _placementController.StartPlacement(_stockpileDef);
            }

            // [4] Dry Granary
            if (GUILayout.Button("[4] Dry Granary\n16T 12S 10Th", _buttonStyle, GUILayout.Width(135), GUILayout.Height(44)))
            {
                if (_granaryDef != null && _placementController != null)
                    _placementController.StartPlacement(_granaryDef);
            }

            // [5] Stone House
            if (GUILayout.Button("[5] Stone House\n14T 20S 8Th", _buttonStyle, GUILayout.Width(135), GUILayout.Height(44)))
            {
                if (_stoneHouseDef != null && _placementController != null)
                    _placementController.StartPlacement(_stoneHouseDef);
            }

            // [6] Watermill
            if (GUILayout.Button("[6] Watermill\n18T 16S", _buttonStyle, GUILayout.Width(135), GUILayout.Height(44)))
            {
                if (_watermillDef != null && _placementController != null)
                    _placementController.StartPlacement(_watermillDef);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawInspectionCard()
        {
            if (_selectedObject == null) return;

            float cardW = 280f;
            float cardH = 220f;
            float startX = Screen.width - cardW - 20f;
            float startY = Screen.height - cardH - 20f;

            GUI.Box(new Rect(startX, startY, cardW, cardH), GUIContent.none, _cardStyle);
            GUILayout.BeginArea(new Rect(startX + 14, startY + 12, cardW - 28, cardH - 24));

            var citizen = _selectedObject.GetComponent<CitizenEntity>();
            if (citizen != null)
            {
                GUILayout.Label($"Citizen: {citizen.CitizenName}", _cardHeaderStyle);
                GUILayout.Label($"Profession: {citizen.Profession}", _statLabelStyle);
                GUILayout.Label($"Action: {citizen.ActionState}", _statLabelStyle);

                if (citizen.Physiology != null)
                {
                    GUILayout.Label($"Health: {citizen.Physiology.Health:0}%", _statLabelStyle);
                    GUILayout.Label($"Hydration: {citizen.Physiology.Hydration:0}%", _statLabelStyle);
                    GUILayout.Label($"Satiety: {citizen.Physiology.Satiety:0}%", _statLabelStyle);
                    GUILayout.Label($"Rest: {citizen.Physiology.Rest:0}%", _statLabelStyle);
                }

                string homeStatus = citizen.HomeDwellingId != 0 ? $"Dwelling #{citizen.HomeDwellingId}" : "Homeless (Chautara Camp)";
                GUILayout.Label($"Shelter: {homeStatus}", _statLabelStyle);

                if (citizen.Inventory != null)
                {
                    GUILayout.Label($"Dokoko Basket: {citizen.Inventory.GetTotalStoredUnits()}/20 units", _statLabelStyle);
                }
            }
            else
            {
                var cattle = _selectedObject.GetComponent<CattleEntity>();
                if (cattle != null)
                {
                    GUILayout.Label($"Cattle: {cattle.CattleName}", _cardHeaderStyle);
                    GUILayout.Label($"Gender: {cattle.Gender}", _statLabelStyle);
                    GUILayout.Label($"Health: {cattle.Health:0}%", _statLabelStyle);

                    if (cattle.Gender == CattleGender.Cow)
                    {
                        GUILayout.Label(cattle.IsReadyToMilk ? "Milk: Ready for Milking!" : "Milk: Replenishing...", _statLabelStyle);
                        if (cattle.IsReadyToMilk)
                        {
                            if (GUILayout.Button("Milk Cow (+4 Milk)", _activeButtonStyle, GUILayout.Height(30)))
                            {
                                int milk = cattle.MilkCow();
                                Debug.Log($"Milked {cattle.CattleName} for {milk} fresh milk!");
                            }
                        }
                    }
                }
                else
                {
                    var site = _selectedObject.GetComponent<ConstructionSite>();
                    if (site != null)
                    {
                        GUILayout.Label($"Construction: {site.Definition.displayName}", _cardHeaderStyle);
                        GUILayout.Label($"Stage: {site.CurrentStage}", _statLabelStyle);
                        GUILayout.Label($"Labor: {site.GetLaborProgress01() * 100f:0}%", _statLabelStyle);
                        GUILayout.Label(site.IsFullyDelivered() ? "Status: Ready for Builders" : "Status: Awaiting Delivery", _statLabelStyle);
                    }
                    else
                    {
                        var node = _selectedObject.GetComponent<NaturalResourceNode>();
                        if (node != null)
                        {
                            GUILayout.Label($"Resource: {node.NodeType}", _cardHeaderStyle);
                            GUILayout.Label($"Yield: {node.RemainingUnits} {node.YieldItem} remaining", _statLabelStyle);
                        }
                        else
                        {
                            var dwelling = _selectedObject.GetComponent<Dewabhumi.Settlement.DwellingEntity>();
                            if (dwelling != null)
                            {
                                GUILayout.Label("Dwelling (Ghar)", _cardHeaderStyle);
                                GUILayout.Label($"Occupancy: {dwelling.OccupancyCount}/{dwelling.BedCapacity} Beds Claimed", _statLabelStyle);
                                GUILayout.Label(dwelling.HasAvailableBed ? "Beds: Has Vacancy" : "Beds: Fully Occupied", _statLabelStyle);
                            }
                            else
                            {
                                var storage = _selectedObject.GetComponent<StockpileEntity>();
                                if (storage != null)
                                {
                                    GUILayout.Label($"Storage: {storage.StorageType}", _cardHeaderStyle);
                                    GUILayout.Label($"Stored Units: {storage.Inventory.GetTotalStoredUnits()}", _statLabelStyle);
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.EndArea();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
