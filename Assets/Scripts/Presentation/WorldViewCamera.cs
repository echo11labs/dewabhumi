using UnityEngine;

namespace Dewabhumi.Presentation
{
    /// <summary>
    /// Smooth 2.5D Isometric Camera Controller.
    /// Strictly complies with docs/art_and_ui/ART_DIRECTION.md and docs/design/SPATIAL_AND_GRID.md.
    /// Locks azimuth to 45 degrees and pitch to 35 degrees.
    /// </summary>
    public class WorldViewCamera : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _panSpeed = 25f;
        [SerializeField] private float _fastPanMultiplier = 2.0f;
        [SerializeField] private float _panSmoothing = 12f;

        [Header("Zoom")]
        [SerializeField] private float _minZoomDistance = 8f;   // Close inspection
        [SerializeField] private float _maxZoomDistance = 90f;  // Valley overview
        [SerializeField] private float _zoomSpeed = 15f;
        [SerializeField] private float _zoomSmoothing = 10f;

        [Header("Bounds")]
        [SerializeField] private Vector2 _minBounds = new Vector2(-200f, -200f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(200f, 200f);

        private Vector3 _targetFocalPoint;
        private float _currentZoomDistance = 30f;
        private float _targetZoomDistance = 30f;

        private Camera _cam;
        private Vector3 _dragOrigin;
        private bool _isDragging = false;

        private const float AzimuthAngle = 45f;
        private const float PitchAngle = 35f;

        private void Awake()
        {
            _cam = GetComponentInChildren<Camera>();
            if (_cam == null)
            {
                _cam = Camera.main;
            }
            _targetFocalPoint = transform.position;
            ApplyRigOrientation();
        }

        private void Start()
        {
            _targetZoomDistance = 30f;
            _currentZoomDistance = 30f;
            UpdateCameraTransformImmediate();
        }

        private void Update()
        {
            HandleKeyboardPan();
            HandleMouseDragPan();
            HandleZoom();
            SmoothUpdateTransforms();
        }

        private void ApplyRigOrientation()
        {
            transform.rotation = Quaternion.Euler(PitchAngle, AzimuthAngle, 0f);
        }

        private void HandleKeyboardPan()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
            {
                // Move relative to camera yaw (45 degrees)
                Vector3 forward = Quaternion.Euler(0, AzimuthAngle, 0) * Vector3.forward;
                Vector3 right = Quaternion.Euler(0, AzimuthAngle, 0) * Vector3.right;

                Vector3 direction = (forward * vertical + right * horizontal).normalized;
                float speed = _panSpeed * (Input.GetKey(KeyCode.LeftShift) ? _fastPanMultiplier : 1f);

                _targetFocalPoint += direction * (speed * Time.deltaTime);
                ClampFocalPoint();
            }
        }

        private void HandleMouseDragPan()
        {
            // Middle mouse or Right mouse drag
            if (Input.GetMouseButtonDown(2) || (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl)))
            {
                _dragOrigin = Input.mousePosition;
                _isDragging = true;
            }

            if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 delta = Input.mousePosition - _dragOrigin;
                _dragOrigin = Input.mousePosition;

                // Project screen delta onto ground plane
                Vector3 forward = Quaternion.Euler(0, AzimuthAngle, 0) * Vector3.forward;
                Vector3 right = Quaternion.Euler(0, AzimuthAngle, 0) * Vector3.right;

                float factor = (_currentZoomDistance / 50f) * 0.05f;
                Vector3 move = (-right * delta.x - forward * delta.y) * factor;

                _targetFocalPoint += move;
                ClampFocalPoint();
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _targetZoomDistance -= scroll * _zoomSpeed * (_currentZoomDistance * 0.3f);
                _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, _minZoomDistance, _maxZoomDistance);
            }
        }

        private void ClampFocalPoint()
        {
            _targetFocalPoint.x = Mathf.Clamp(_targetFocalPoint.x, _minBounds.x, _maxBounds.x);
            _targetFocalPoint.z = Mathf.Clamp(_targetFocalPoint.z, _minBounds.y, _maxBounds.y);
        }

        private void SmoothUpdateTransforms()
        {
            transform.position = Vector3.Lerp(transform.position, _targetFocalPoint, Time.deltaTime * _panSmoothing);
            _currentZoomDistance = Mathf.Lerp(_currentZoomDistance, _targetZoomDistance, Time.deltaTime * _zoomSmoothing);

            if (_cam != null)
            {
                _cam.transform.localPosition = new Vector3(0f, 0f, -_currentZoomDistance);
                _cam.transform.localRotation = Quaternion.identity;
            }
        }

        private void UpdateCameraTransformImmediate()
        {
            transform.position = _targetFocalPoint;
            if (_cam != null)
            {
                _cam.transform.localPosition = new Vector3(0f, 0f, -_currentZoomDistance);
                _cam.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
