using System;
using System.Collections.Generic;
using UnityEngine;
using Dewabhumi.Citizens;

namespace Dewabhumi.Settlement
{
    /// <summary>
    /// Component managing a residential dwelling (e.g. Simple Ghar, Stone House).
    /// Tracks bed allocations, resident occupancy, and night rest shelter.
    /// Strictly complies with docs/design/SETTLEMENT_AND_BUILDINGS.md (BLD-04).
    /// </summary>
    public class DwellingEntity : MonoBehaviour
    {
        public ulong EntityId { get; private set; }
        public int BedCapacity { get; private set; } = 4;
        public Vector3 WorldPosition { get; private set; }

        private readonly List<ulong> _residentIds = new List<ulong>();

        public IReadOnlyList<ulong> Residents => _residentIds;
        public int OccupancyCount => _residentIds.Count;
        public bool HasAvailableBed => _residentIds.Count < BedCapacity;

        public void Initialize(ulong entityId, int bedCapacity, Vector3 worldPosition)
        {
            EntityId = entityId;
            BedCapacity = bedCapacity;
            WorldPosition = worldPosition;
            transform.position = worldPosition;
        }

        public bool TryAssignResident(ulong citizenId)
        {
            if (_residentIds.Count >= BedCapacity) return false;
            if (!_residentIds.Contains(citizenId))
            {
                _residentIds.Add(citizenId);
                Debug.Log($"[DwellingEntity] Citizen #{citizenId} assigned bed in Dwelling #{EntityId} ({_residentIds.Count}/{BedCapacity}).");
                return true;
            }
            return false;
        }

        public void RemoveResident(ulong citizenId)
        {
            if (_residentIds.Remove(citizenId))
            {
                Debug.Log($"[DwellingEntity] Citizen #{citizenId} vacated bed in Dwelling #{EntityId}.");
            }
        }
    }
}
