using System;
using System.Collections.Generic;

namespace Dewabhumi.Economy
{
    /// <summary>
    /// Pure C# discrete integer inventory component.
    /// Strictly complies with docs/design/ECONOMY_AND_RESOURCES.md (SIM-04).
    /// Guarantees atomic transfers and zero duplication / phantom creation.
    /// </summary>
    public class InventoryComponent
    {
        public ulong OwnerEntityId { get; }
        public int CapacityUnits { get; set; }

        private readonly Dictionary<ItemType, int> _items = new Dictionary<ItemType, int>();
        private readonly Dictionary<ItemType, int> _reserved = new Dictionary<ItemType, int>();

        public InventoryComponent(ulong ownerEntityId, int capacityUnits)
        {
            OwnerEntityId = ownerEntityId;
            CapacityUnits = capacityUnits;
        }

        public int GetStoredQuantity(ItemType item)
        {
            return _items.TryGetValue(item, out int q) ? q : 0;
        }

        public int GetReservedQuantity(ItemType item)
        {
            return _reserved.TryGetValue(item, out int r) ? r : 0;
        }

        public int GetAvailableQuantity(ItemType item)
        {
            return GetStoredQuantity(item) - GetReservedQuantity(item);
        }

        public int GetTotalStoredUnits()
        {
            int sum = 0;
            foreach (var kvp in _items) sum += kvp.Value;
            return sum;
        }

        public int GetRemainingCapacity()
        {
            return Math.Max(0, CapacityUnits - GetTotalStoredUnits());
        }

        public bool CanAccept(ItemType item, int amount)
        {
            if (amount <= 0) return true;
            return GetRemainingCapacity() >= amount;
        }

        /// <summary>
        /// Adds resources directly into inventory. Used only for validated generation (harvesting, production).
        /// </summary>
        public bool DepositDirect(ItemType item, int amount)
        {
            if (amount <= 0) return false;
            if (!CanAccept(item, amount)) return false;

            _items[item] = GetStoredQuantity(item) + amount;
            return true;
        }

        /// <summary>
        /// Removes unreserved resources directly from inventory. Used for consumption or decay.
        /// </summary>
        public bool WithdrawDirect(ItemType item, int amount)
        {
            if (amount <= 0) return false;
            if (GetAvailableQuantity(item) < amount) return false;

            _items[item] -= amount;
            if (_items[item] == 0) _items.Remove(item);
            return true;
        }

        public bool TryWithdraw(ItemType item, int amount)
        {
            return WithdrawDirect(item, amount);
        }

        /// <summary>
        /// Reserves an amount for an upcoming pickup by another entity (e.g. hauler or builder).
        /// </summary>
        public bool Reserve(ItemType item, int amount)
        {
            if (amount <= 0) return false;
            if (GetAvailableQuantity(item) < amount) return false;

            _reserved[item] = GetReservedQuantity(item) + amount;
            return true;
        }

        /// <summary>
        /// Releases a previously locked reservation back into available stock.
        /// </summary>
        public bool ReleaseReservation(ItemType item, int amount)
        {
            if (amount <= 0) return false;
            int currentReserved = GetReservedQuantity(item);
            if (currentReserved < amount) return false;

            _reserved[item] -= amount;
            if (_reserved[item] == 0) _reserved.Remove(item);
            return true;
        }

        /// <summary>
        /// Transfers an amount from this inventory to destination atomically.
        /// If wasReserved is true, decrements the reservation on the source inventory.
        /// </summary>
        public static bool TransferAtomic(InventoryComponent source, InventoryComponent destination, ItemType item, int amount, bool wasReserved)
        {
            if (amount <= 0) return false;
            if (!destination.CanAccept(item, amount)) return false;

            if (wasReserved)
            {
                if (source.GetReservedQuantity(item) < amount) return false;
                if (source.GetStoredQuantity(item) < amount) return false;

                source._reserved[item] -= amount;
                if (source._reserved[item] == 0) source._reserved.Remove(item);

                source._items[item] -= amount;
                if (source._items[item] == 0) source._items.Remove(item);
            }
            else
            {
                if (source.GetAvailableQuantity(item) < amount) return false;

                source._items[item] -= amount;
                if (source._items[item] == 0) source._items.Remove(item);
            }

            destination._items[item] = destination.GetStoredQuantity(item) + amount;
            return true;
        }
    }
}
