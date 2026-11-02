using System;
using UnityEngine;

namespace Dewabhumi.Spatial
{
    /// <summary>
    /// Authoritative service governing conversions between continuous 3D world coordinates
    /// and the 2.0-metre discrete logical settlement grid.
    /// Strictly complies with docs/design/SPATIAL_AND_GRID.md.
    /// </summary>
    public static class SpatialCoordinateService
    {
        public const float CellSize = 2.0f;
        public const float HalfCellSize = CellSize * 0.5f;

        /// <summary>
        /// Converts a world-space position into discrete integer grid coordinates (X, Z).
        /// </summary>
        public static Vector2Int WorldToGridCell(Vector3 worldPosition)
        {
            int cellX = Mathf.FloorToInt(worldPosition.x / CellSize);
            int cellZ = Mathf.FloorToInt(worldPosition.z / CellSize);
            return new Vector2Int(cellX, cellZ);
        }

        /// <summary>
        /// Returns the world-space center point of a given discrete grid cell.
        /// </summary>
        public static Vector3 GridCellToWorldCenter(Vector2Int gridCell, float yHeight = 0f)
        {
            float worldX = (gridCell.x * CellSize) + HalfCellSize;
            float worldZ = (gridCell.y * CellSize) + HalfCellSize;
            return new Vector3(worldX, yHeight, worldZ);
        }

        /// <summary>
        /// Returns the world-space south-west corner of a given discrete grid cell.
        /// </summary>
        public static Vector3 GridCellToWorldCorner(Vector2Int gridCell, float yHeight = 0f)
        {
            float worldX = gridCell.x * CellSize;
            float worldZ = gridCell.y * CellSize;
            return new Vector3(worldX, yHeight, worldZ);
        }

        /// <summary>
        /// Snaps an arbitrary world position to the nearest 2m grid cell center.
        /// </summary>
        public static Vector3 SnapToGridCenter(Vector3 worldPosition)
        {
            Vector2Int cell = WorldToGridCell(worldPosition);
            return GridCellToWorldCenter(cell, worldPosition.y);
        }

        /// <summary>
        /// Clamps rotation angles to strict 90-degree orthogonal increments (0, 90, 180, 270).
        /// </summary>
        public static int SnapOrthogonalRotation(float angleDegrees)
        {
            int normalized = Mathf.RoundToInt(angleDegrees / 90f) % 4;
            if (normalized < 0) normalized += 4;
            return normalized * 90;
        }

        /// <summary>
        /// Calculates the grid cell footprint bounds for a structure with given dimensions and 90-deg rotation.
        /// </summary>
        public static (int widthCells, int depthCells) GetRotatedFootprint(int baseWidthCells, int baseDepthCells, int rotationDegrees)
        {
            int normalized = (rotationDegrees / 90) % 2;
            return normalized == 0 ? (baseWidthCells, baseDepthCells) : (baseDepthCells, baseWidthCells);
        }
    }
}
