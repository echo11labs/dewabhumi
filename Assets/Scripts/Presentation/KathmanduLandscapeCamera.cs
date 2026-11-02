using UnityEngine;

namespace Dewabhumi.Presentation
{
    /// <summary>Navigation for the geographic landscape preview; no simulation dependencies.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class KathmanduLandscapeCamera : MonoBehaviour
    {
        [SerializeField] private Terrain _regionalTerrain;
        [SerializeField] private Terrain _localTerrain;
        [SerializeField] private Vector3 _focus;
        [SerializeField] private float _distance = 2400f;
        [SerializeField] private float _yaw = 15f;
        [SerializeField] private float _pitch = 52f;
        private Camera _camera;

        public void Configure(Terrain regional, Terrain local, Vector3 focus)
        {
            _regionalTerrain = regional;
            _localTerrain = local;
            _focus = focus;
            ApplyView();
        }

        public void SetView(bool overview)
        {
            _focus = overview ? new Vector3(0, Ground(0, 0), 0) : new Vector3(-7490, Ground(-7490, -1230), -1230);
            _distance = overview ? 65000f : 2400f;
            _yaw = overview ? 0 : 15;
            _pitch = overview ? 65 : 52;
            ApplyView();
        }

        public void SetCloseView()
        {
            SetView(false);
            _distance = 180;
            _pitch = 42;
            ApplyView();
        }

        public void SetSurfaceView()
        {
            SetView(false);
            _focus.x += 65;
            _focus.y = Ground(_focus.x, _focus.z);
            _distance = 55;
            _pitch = 38;
            ApplyView();
        }

        private void Awake() { ApplyView(); }

        private float Ground(float x, float z)
        {
            Terrain terrain = _regionalTerrain;
            if (_localTerrain != null)
            {
                Vector3 p = _localTerrain.transform.position;
                Vector3 size = _localTerrain.terrainData.size;
                if (x >= p.x && x <= p.x + size.x && z >= p.z && z <= p.z + size.z) terrain = _localTerrain;
            }
            return terrain == null ? 1300 : terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O)) SetView(true);
            if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.Home)) SetView(false);
            if (Input.GetKeyDown(KeyCode.C)) SetCloseView();
            if (Input.GetKeyDown(KeyCode.V)) SetSurfaceView();
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * 3f;
                _pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * 2f, 25, 85);
            }
            if (Input.GetKey(KeyCode.Q)) _yaw -= 45 * Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.E)) _yaw += 45 * Time.unscaledDeltaTime;
            Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            if (Input.GetMouseButton(2)) direction += new Vector3(-Input.GetAxis("Mouse X") * 3, 0, -Input.GetAxis("Mouse Y") * 3);
            _focus += Quaternion.Euler(0, _yaw, 0) * direction * (_distance * 0.3f * Time.unscaledDeltaTime * (Input.GetKey(KeyCode.LeftShift) ? 3 : 1));
            _focus.x = Mathf.Clamp(_focus.x, -19500, 19500);
            _focus.z = Mathf.Clamp(_focus.z, -19500, 19500);
            _focus.y = Ground(_focus.x, _focus.z);
            _distance = Mathf.Clamp(_distance * Mathf.Exp(-Input.mouseScrollDelta.y * 0.12f), 25, 65000);
            ApplyView();
        }

        private void ApplyView()
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            transform.SetPositionAndRotation(_focus - rotation * Vector3.forward * _distance, rotation);
            _camera.nearClipPlane = Mathf.Max(0.3f, _distance / 1500);
            _camera.farClipPlane = 100000;
            _camera.fieldOfView = 42;
        }

        private void OnGUI()
        {
            float scale = Mathf.Clamp(Screen.width / 1600f, 0.8f, 1.5f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            GUI.Box(new Rect(20, 20, 380, 92), GUIContent.none);
            GUI.Label(new Rect(35, 28, 350, 26), "DEWABHUMI  /  THE GOPALA LANDSCAPE");
            GUI.Label(new Rect(35, 55, 350, 22), "Kathmandu Valley · early pastoral interpretation");
            GUI.Label(new Rect(35, 80, 350, 22), "O Valley   L Settlement   C Banks   V Surface");
            GUI.Label(new Rect(25, Screen.height / scale - 40, 1000, 25), "WASD / middle drag: pan    Wheel: zoom    Right drag / Q,E: orbit    Shift: move faster");
            GUI.matrix = previous;
        }
    }
}
