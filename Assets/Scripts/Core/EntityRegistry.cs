using System;
using System.Collections.Generic;

namespace Dewabhumi.Core
{
    public enum EntityType
    {
        Villager = 0,
        Cattle = 1,
        Building = 2,
        ConstructionSite = 3,
        Stockpile = 4,
        ResourceNode = 5
    }

    /// <summary>
    /// Centralized registry managing stable 64-bit EntityIds and active entity records.
    /// Strictly complies with docs/design/SIMULATION_ARCHITECTURE.md (SIM-02).
    /// </summary>
    public class EntityRegistry
    {
        private ulong _nextEntityId = 1000;
        private readonly Dictionary<ulong, EntityType> _entityTypes = new Dictionary<ulong, EntityType>();

        public ulong AllocateEntityId(EntityType type)
        {
            ulong id = _nextEntityId++;
            _entityTypes[id] = type;
            return id;
        }

        public bool RegisterEntityWithId(ulong id, EntityType type)
        {
            if (_entityTypes.ContainsKey(id))
            {
                return false;
            }
            _entityTypes[id] = type;
            if (id >= _nextEntityId)
            {
                _nextEntityId = id + 1;
            }
            return true;
        }

        public bool IsAlive(ulong id)
        {
            return _entityTypes.ContainsKey(id);
        }

        public EntityType GetEntityType(ulong id)
        {
            if (_entityTypes.TryGetValue(id, out var type))
            {
                return type;
            }
            throw new KeyNotFoundException($"Entity ID {id} not found in registry.");
        }

        public bool UnregisterEntity(ulong id)
        {
            return _entityTypes.Remove(id);
        }

        public void Clear()
        {
            _entityTypes.Clear();
            _nextEntityId = 1000;
        }
    }
}
