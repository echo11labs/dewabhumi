using System.Collections.Generic;
using UnityEngine;

namespace Dewabhumi.Settlement
{
    /// <summary>
    /// Tracks spatial occupancy on the 2.0-metre logical settlement grid.
    /// Prevents overlapping buildings, blocked doors, and duplicate placement.
    /// Strictly complies with docs/design/SPATIAL_AND_GRID.md.
    /// </summary>
    public class OccupancyGrid
    {
        private readonly Dictionary<Vector2Int, ulong> _occupiedCells = new Dictionary<Vector2Int, ulong>();

        public bool IsCellOccupied(Vector2Int cell)
        {
            return _occupiedCells.ContainsKey(cell);
        }

        public bool TryGetOccupant(Vector2Int cell, out ulong entityId)
        {
            return _occupiedCells.TryGetValue(cell, out entityId);
        }

        public bool CanOccupyArea(Vector2Int originCell, Vector2Int sizeCells)
        {
            for (int z = 0; z < sizeCells.y; z++)
            {
                for (int x = 0; x < sizeCells.x; x++)
                {
                    Vector2Int checkCell = new Vector2Int(originCell.x + x, originCell.y + z);
                    if (_occupiedCells.ContainsKey(checkCell))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void OccupyArea(Vector2Int originCell, Vector2Int sizeCells, ulong entityId)
        {
            for (int z = 0; z < sizeCells.y; z++)
            {
                for (int x = 0; x < sizeCells.x; x++)
                {
                    Vector2Int cell = new Vector2Int(originCell.x + x, originCell.y + z);
                    _occupiedCells[cell] = entityId;
                }
            }
        }

        public void FreeArea(Vector2Int originCell, Vector2Int sizeCells)
        {
            for (int z = 0; z < sizeCells.y; z++)
            {
                for (int x = 0; x < sizeCells.x; x++)
                {
                    Vector2Int cell = new Vector2Int(originCell.x + x, originCell.y + z);
                    _occupiedCells.Remove(cell);
                }
            }
        }

        public void Clear()
        {
            _occupiedCells.Clear();
        }
    }
}
