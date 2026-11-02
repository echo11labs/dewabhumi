using UnityEngine;
using Dewabhumi.Spatial;

namespace Dewabhumi.Presentation
{
    /// <summary>
    /// Visualizes the 2.0-metre logical settlement grid and highlights hovered grid cells.
    /// Strictly complies with docs/design/SPATIAL_AND_GRID.md.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visualization")]
        [SerializeField] private bool _showGrid = true;
        [SerializeField] private Color _gridColor = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _hoverValidColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        [SerializeField] private Color _hoverInvalidColor = new Color(0.9f, 0.2f, 0.2f, 0.4f);

        [Header("Grid Extents")]
        [SerializeField] private int _gridRadiusCells = 24; // 48 x 48 cells (96m x 96m)

        public Vector2Int HoveredCell { get; private set; }
        public bool IsHoveringValidTerrain { get; private set; }

        private Camera _mainCam;
        private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        private void Start()
        {
            _mainCam = Camera.main;
        }

        private void Update()
        {
            UpdateHoveredCell();

            // Toggle grid with 'G'
            if (Input.GetKeyDown(KeyCode.G))
            {
                _showGrid = !_showGrid;
            }
        }

        private void UpdateHoveredCell()
        {
            if (_mainCam == null) return;

            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            if (_groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                HoveredCell = SpatialCoordinateService.WorldToGridCell(hitPoint);
                IsHoveringValidTerrain = true;
            }
            else
            {
                IsHoveringValidTerrain = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showGrid) return;

            Gizmos.color = _gridColor;
            float size = SpatialCoordinateService.CellSize;
            float halfRadius = _gridRadiusCells * size;

            Vector3 center = transform.position;
            Vector2Int centerCell = SpatialCoordinateService.WorldToGridCell(center);
            Vector3 gridCenter = SpatialCoordinateService.GridCellToWorldCorner(centerCell);

            // Draw horizontal and vertical grid lines
            for (int i = -_gridRadiusCells; i <= _gridRadiusCells; i++)
            {
                float x = gridCenter.x + (i * size);
                Gizmos.DrawLine(new Vector3(x, 0.02f, gridCenter.z - halfRadius), new Vector3(x, 0.02f, gridCenter.z + halfRadius));

                float z = gridCenter.z + (i * size);
                Gizmos.DrawLine(new Vector3(gridCenter.x - halfRadius, 0.02f, z), new Vector3(gridCenter.x + halfRadius, 0.02f, z));
            }

            // Draw highlighted hover cell
            if (Application.isPlaying && IsHoveringValidTerrain)
            {
                Gizmos.color = _hoverValidColor;
                Vector3 cellCenter = SpatialCoordinateService.GridCellToWorldCenter(HoveredCell, 0.05f);
                Gizmos.DrawCube(cellCenter, new Vector3(size * 0.95f, 0.04f, size * 0.95f));
            }
        }
    }
}
