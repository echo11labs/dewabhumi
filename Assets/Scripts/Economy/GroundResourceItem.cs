using UnityEngine;
using Dewabhumi.Spatial;

namespace Dewabhumi.Economy
{
    /// <summary>
    /// Represents a physically dropped harvest unit on the ground awaiting transport.
    /// Strictly complies with docs/design/ECONOMY_AND_RESOURCES.md (Conservation Invariant).
    /// </summary>
    public class GroundResourceItem : MonoBehaviour
    {
        public ulong EntityId { get; private set; }
        public ItemType ItemType { get; private set; }
        public int Amount { get; private set; } = 1;
        public bool IsReserved { get; private set; } = false;
        public ulong ReservedByWorkerId { get; private set; } = 0;

        public Vector2Int GridCell { get; private set; }

        public void Initialize(ulong entityId, ItemType itemType, int amount, Vector3 worldPosition)
        {
            EntityId = entityId;
            ItemType = itemType;
            Amount = amount;

            transform.position = worldPosition;
            GridCell = SpatialCoordinateService.WorldToGridCell(worldPosition);

            CreateVisual();
        }

        public bool TryReserve(ulong workerId)
        {
            if (IsReserved) return false;
            IsReserved = true;
            ReservedByWorkerId = workerId;
            return true;
        }

        public void ReleaseReservation()
        {
            IsReserved = false;
            ReservedByWorkerId = 0;
        }

        private void CreateVisual()
        {
            GameObject visual = null;
            switch (ItemType)
            {
                case ItemType.Timber:
                    // 2.0m cut log
                    visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    visual.transform.localScale = new Vector3(0.30f, 1.0f, 0.30f);
                    visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    visual.GetComponent<Renderer>().material.color = new Color(0.38f, 0.25f, 0.15f);
                    break;
                case ItemType.LooseStone:
                case ItemType.StoneBlock:
                    // Granite block
                    visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visual.transform.localScale = new Vector3(0.40f, 0.25f, 0.30f);
                    visual.GetComponent<Renderer>().material.color = new Color(0.55f, 0.58f, 0.60f);
                    break;
                default:
                    visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visual.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    visual.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
            }

            visual.name = "Visual_GroundItem";
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            var col = visual.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
    }
}
