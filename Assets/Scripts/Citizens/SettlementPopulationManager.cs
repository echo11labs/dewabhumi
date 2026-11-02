using System;
using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Core;
using Dewabhumi.Pastoral;

namespace Dewabhumi.Citizens
{
    /// <summary>
    /// Central manager for citizen populations, cattle herds, and simulation tick propagation.
    /// Strictly complies with docs/design/CITIZENS_AND_LABOR.md and docs/design/HERD_AND_CATTLE.md.
    /// </summary>
    public class SettlementPopulationManager : MonoBehaviour
    {
        public static SettlementPopulationManager Instance { get; private set; }

        [Header("Starting Population")]
        [SerializeField] private bool _spawnFoundingClanOnStart = true;
        [SerializeField] private Vector3 _settlementCenterWorld = new Vector3(256f, 15.5f, 230f);

        private readonly List<CitizenEntity> _citizens = new List<CitizenEntity>();
        private readonly List<CattleEntity> _cattle = new List<CattleEntity>();
        private readonly List<Dewabhumi.Settlement.DwellingEntity> _dwellings = new List<Dewabhumi.Settlement.DwellingEntity>();

        private SimulationClock _simulationClock;
        private EntityRegistry _entityRegistry;

        public IReadOnlyList<CitizenEntity> Citizens => _citizens;
        public IReadOnlyList<CattleEntity> Cattle => _cattle;
        public IReadOnlyList<Dewabhumi.Settlement.DwellingEntity> Dwellings => _dwellings;
        public SimulationClock Clock => _simulationClock;

        public int TotalPopulation => _citizens.Count;
        public int ActiveWorkers => _citizens.FindAll(c => c.Physiology.IsAlive).Count;
        public int HomelessCount => _citizens.FindAll(c => c.HomeDwellingId == 0).Count;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            _entityRegistry = new EntityRegistry();
            _simulationClock = new SimulationClock();
        }

        private void Start()
        {
            // Subscribe to clock ticks
            _simulationClock.OnTick += HandleSimulationTick;

            if (_spawnFoundingClanOnStart)
            {
                if (transform.position.sqrMagnitude > 1f)
                {
                    _settlementCenterWorld = transform.position;
                }
                SpawnFoundingClan();
            }
        }

        private void Update()
        {
            // Drive deterministic clock with delta time
            _simulationClock.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_simulationClock != null)
            {
                _simulationClock.OnTick -= HandleSimulationTick;
            }
        }

        private void HandleSimulationTick(ulong tickNumber)
        {
            // Tick all citizens
            for (int i = 0; i < _citizens.Count; i++)
            {
                _citizens[i].SimulationTick(tickNumber);
            }

            // Tick all cattle
            for (int i = 0; i < _cattle.Count; i++)
            {
                _cattle[i].SimulationTick(tickNumber);
            }
        }

        public void SpawnFoundingClan()
        {
            // 8 Founding Settlers (docs/GDD.md MVP spec)
            var foundingRoster = new (string name, ProfessionType profession, Vector2 offset)[]
            {
                ("Devendra", ProfessionType.Builder, new Vector2(-2f, -1f)),
                ("Pasang", ProfessionType.Woodcutter, new Vector2(3f, 2f)),
                ("Tashi", ProfessionType.Stonemason, new Vector2(-4f, 3f)),
                ("Maya", ProfessionType.Forager, new Vector2(2f, -3f)),
                ("Pemba", ProfessionType.Herder, new Vector2(5f, -1f)),
                ("Kanchi", ProfessionType.Farmer, new Vector2(-1f, 4f)),
                ("Bikram", ProfessionType.WaterCarrier, new Vector2(4f, -4f)),
                ("Sita", ProfessionType.GeneralLaborer, new Vector2(-3f, -3f))
            };

            Terrain terrain = Terrain.activeTerrain;

            foreach (var member in foundingRoster)
            {
                ulong id = _entityRegistry.AllocateEntityId(EntityType.Villager);
                Vector3 spawnPos = _settlementCenterWorld + new Vector3(member.offset.x, 0f, member.offset.y);
                if (terrain != null)
                {
                    spawnPos.y = terrain.SampleHeight(spawnPos);
                }

                GameObject citizenObj = new GameObject($"Citizen_{member.name}_{id}");
                citizenObj.transform.SetParent(transform);

                CitizenEntity citizen = citizenObj.AddComponent<CitizenEntity>();
                citizen.Initialize(id, member.name, member.profession, spawnPos);
                _citizens.Add(citizen);
            }

            // 2 Ancestral Zebu Cattle (docs/design/HERD_AND_CATTLE.md)
            var foundingCattle = new (string name, CattleGender gender, Vector2 offset)[]
            {
                ("Kamadhenu", CattleGender.Cow, new Vector2(8f, 4f)),
                ("Nandi", CattleGender.Bull, new Vector2(10f, 6f))
            };

            foreach (var cow in foundingCattle)
            {
                ulong id = _entityRegistry.AllocateEntityId(EntityType.Cattle);
                Vector3 spawnPos = _settlementCenterWorld + new Vector3(cow.offset.x, 0f, cow.offset.y);
                if (terrain != null)
                {
                    spawnPos.y = terrain.SampleHeight(spawnPos);
                }

                GameObject cattleObj = new GameObject($"Cattle_{cow.name}_{id}");
                cattleObj.transform.SetParent(transform);

                CattleEntity cattle = cattleObj.AddComponent<CattleEntity>();
                cattle.Initialize(id, cow.name, cow.gender, spawnPos);
                _cattle.Add(cattle);
            }

            Debug.Log($"[SettlementPopulationManager] Successfully spawned 8 founding citizens and 2 Zebu cattle at {_settlementCenterWorld}.");
        }

        public void RegisterDwelling(Dewabhumi.Settlement.DwellingEntity dwelling)
        {
            if (!_dwellings.Contains(dwelling))
            {
                _dwellings.Add(dwelling);
                AssignHomelessToDwelling(dwelling);
            }
        }

        public void AssignHomelessToDwelling(Dewabhumi.Settlement.DwellingEntity dwelling)
        {
            if (dwelling == null) return;

            for (int i = 0; i < _citizens.Count; i++)
            {
                if (!dwelling.HasAvailableBed) break;

                var c = _citizens[i];
                if (c.HomeDwellingId == 0)
                {
                    if (dwelling.TryAssignResident(c.EntityId))
                    {
                        c.AssignDwelling(dwelling.EntityId);
                    }
                }
            }
        }

        public Dewabhumi.Settlement.DwellingEntity GetDwelling(ulong dwellingId)
        {
            return _dwellings.Find(d => d.EntityId == dwellingId);
        }
    }
}
