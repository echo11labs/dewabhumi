using System;
using UnityEngine;
using Dewabhumi.Economy;
using Dewabhumi.Spatial;

namespace Dewabhumi.Pastoral
{
    public enum CattleGender
    {
        Cow = 0,
        Bull = 1,
        Calf = 2
    }

    /// <summary>
    /// Component representing an ancestral Zebu cattle entity in Dewabhumi.
    /// Strictly complies with docs/design/HERD_AND_CATTLE.md and docs/art_and_ui/ASSET_PIPELINE.md.
    /// </summary>
    public class CattleEntity : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private ulong _entityId;
        [SerializeField] private string _cattleName = "Zebu Cow";
        [SerializeField] private CattleGender _gender = CattleGender.Cow;

        [Header("Pasture & Grazing")]
        [SerializeField] private float _hunger = 0f; // 0 = Full, 100 = Starving
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _milkReadiness = 1.0f; // 1.0 = Ready for milking at dawn
        [SerializeField] private float _wanderRadius = 16f;
        [SerializeField] private float _walkSpeed = 0.8f;

        public ulong EntityId => _entityId;
        public string CattleName => _cattleName;
        public CattleGender Gender => _gender;
        public float Health => _health;
        public bool IsReadyToMilk => _gender == CattleGender.Cow && _milkReadiness >= 1.0f && _health > 70f;

        private Vector3 _pastureCenter;
        private Vector3 _currentDestination;
        private bool _isMoving = false;
        private float _wanderTimer = 0f;
        private Terrain _terrain;

        public void Initialize(ulong entityId, string name, CattleGender gender, Vector3 startPos)
        {
            _entityId = entityId;
            _cattleName = name;
            _gender = gender;

            _pastureCenter = startPos;
            transform.position = startPos;
            _currentDestination = startPos;

            _terrain = Terrain.activeTerrain;

            CreatePlaceholderVisual();
        }

        private void Update()
        {
            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0f && !_isMoving)
            {
                PickNewWanderPoint();
            }

            if (_isMoving)
            {
                MoveTowardsDestination();
            }
        }

        /// <summary>
        /// Called on each 10 Hz fixed simulation tick.
        /// </summary>
        public void SimulationTick(ulong tickNumber)
        {
            // Graze grass or increase hunger
            _hunger = Mathf.Min(100f, _hunger + (100f / 14400f)); // 1 day to full hunger

            if (_hunger > 80f)
            {
                _health = Mathf.Max(0f, _health - (30f / 14400f));
            }
            else if (_health < 100f)
            {
                _health = Mathf.Min(100f, _health + (10f / 14400f));
            }

            // Restore milk readiness over 1 day
            if (_gender == CattleGender.Cow && _milkReadiness < 1.0f)
            {
                _milkReadiness += (1.0f / 14400f);
            }
        }

        public int MilkCow()
        {
            if (!IsReadyToMilk) return 0;

            _milkReadiness = 0f;
            return 4; // Yields 4 units of FreshMilk per docs/design/HERD_AND_CATTLE.md
        }

        private void PickNewWanderPoint()
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * _wanderRadius;
            Vector3 target = new Vector3(_pastureCenter.x + randomCircle.x, 0f, _pastureCenter.z + randomCircle.y);

            if (_terrain != null)
            {
                target.y = _terrain.SampleHeight(target);
            }

            _currentDestination = target;
            _isMoving = true;
            _wanderTimer = UnityEngine.Random.Range(4f, 10f);
        }

        private void MoveTowardsDestination()
        {
            Vector3 currentPos = transform.position;
            Vector3 target = _currentDestination;

            if (_terrain != null)
            {
                target.y = _terrain.SampleHeight(target);
                currentPos.y = _terrain.SampleHeight(currentPos);
            }

            Vector3 toTarget = target - currentPos;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance <= 0.2f)
            {
                transform.position = target;
                _isMoving = false;
                return;
            }

            Vector3 step = toTarget.normalized * (_walkSpeed * Time.deltaTime);
            if (step.magnitude > distance) step = toTarget;

            Vector3 newPos = currentPos + step;
            if (_terrain != null)
            {
                newPos.y = _terrain.SampleHeight(newPos);
            }

            transform.position = newPos;

            if (toTarget.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget.normalized, Vector3.up), Time.deltaTime * 3f);
            }
        }

        private void CreatePlaceholderVisual()
        {
            // Zebu Cattle metric scale: 0.8m width, 1.4m shoulder height, 2.2m length
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Visual_CattleBody";
            body.transform.SetParent(transform, false);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Horizontal body
            body.transform.localScale = new Vector3(0.8f, 1.1f, 0.7f);
            body.transform.localPosition = new Vector3(0f, 1.0f, 0f);

            // Zebu Characteristic Hump
            GameObject hump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hump.name = "Visual_ZebuHump";
            hump.transform.SetParent(transform, false);
            hump.transform.localScale = new Vector3(0.55f, 0.45f, 0.50f);
            hump.transform.localPosition = new Vector3(0f, 1.55f, 0.45f); // Forward over shoulders

            // Head and Muzzle
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Visual_CattleHead";
            head.transform.SetParent(transform, false);
            head.transform.localScale = new Vector3(0.40f, 0.45f, 0.65f);
            head.transform.localPosition = new Vector3(0f, 1.25f, 1.25f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = _gender == CattleGender.Bull ? new Color(0.35f, 0.32f, 0.30f) : new Color(0.82f, 0.78f, 0.72f); // White/cream cow or grey bull

            body.GetComponent<Renderer>().material = mat;
            hump.GetComponent<Renderer>().material = mat;
            head.GetComponent<Renderer>().material = mat;

            // Remove colliders
            Destroy(body.GetComponent<Collider>());
            Destroy(hump.GetComponent<Collider>());
            Destroy(head.GetComponent<Collider>());
        }
    }
}
