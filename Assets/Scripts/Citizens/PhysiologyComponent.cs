using System;
using UnityEngine;

namespace Dewabhumi.Citizens
{
    /// <summary>
    /// Pure C# physiological state machine for human settlers.
    /// Strictly complies with docs/design/CITIZENS_AND_LABOR.md (CIT-01).
    /// </summary>
    public class PhysiologyComponent
    {
        public const float MaxValue = 100f;
        public const int TicksPerDay = 14400; // 10 ticks/sec * 60 * 60 * 24

        // Decay per tick: 100 units consumed over 1 full day (14400 ticks)
        private const float HydrationDecayPerTick = MaxValue / TicksPerDay;
        private const float SatietyDecayPerTick = MaxValue / TicksPerDay;
        private const float RestDecayPerTick = (MaxValue / (TicksPerDay * 0.75f)); // Exhausted after 18h awake
        private const float RestRecoverPerTick = (MaxValue / (TicksPerDay * 0.25f)); // Fully rested after 6h sleep

        // Health damage per tick when starving/dehydrated (50% health loss per day without water/food)
        private const float StarvationDamagePerTick = (50f / TicksPerDay);
        private const float HealthRecoverPerTick = (20f / TicksPerDay);

        public float Hydration { get; private set; } = 100f;
        public float Satiety { get; private set; } = 100f;
        public float Rest { get; private set; } = 100f;
        public float Health { get; private set; } = 100f;

        public bool IsAlive => Health > 0f;
        public bool IsThirsty => Hydration < 30f;
        public bool IsHungry => Satiety < 30f;
        public bool IsExhausted => Rest < 20f;
        public bool IsCritical => Hydration <= 0f || Satiety <= 0f;

        public event Action OnDied;
        public event Action OnBecameCritical;

        public void Tick(bool isSleeping)
        {
            if (!IsAlive) return;

            // 1. Decay Hydration & Satiety
            Hydration = Mathf.Max(0f, Hydration - HydrationDecayPerTick);
            Satiety = Mathf.Max(0f, Satiety - SatietyDecayPerTick);

            // 2. Rest mechanics
            if (isSleeping)
            {
                Rest = Mathf.Min(MaxValue, Rest + RestRecoverPerTick);
            }
            else
            {
                Rest = Mathf.Max(0f, Rest - RestDecayPerTick);
            }

            // 3. Health consequences
            if (Hydration <= 0f || Satiety <= 0f)
            {
                Health = Mathf.Max(0f, Health - StarvationDamagePerTick);
                OnBecameCritical?.Invoke();

                if (Health <= 0f)
                {
                    OnDied?.Invoke();
                }
            }
            else if (Hydration > 70f && Satiety > 70f && Rest > 50f && Health < MaxValue)
            {
                Health = Mathf.Min(MaxValue, Health + HealthRecoverPerTick);
            }
        }

        public void Drink(float hydrationAmount = 60f)
        {
            Hydration = Mathf.Min(MaxValue, Hydration + hydrationAmount);
        }

        public void Eat(float satietyAmount = 50f)
        {
            Satiety = Mathf.Min(MaxValue, Satiety + satietyAmount);
        }

        public void Heal(float amount)
        {
            if (IsAlive)
            {
                Health = Mathf.Min(MaxValue, Health + amount);
            }
        }

        public void SetState(float hydration, float satiety, float rest, float health)
        {
            Hydration = Mathf.Clamp(hydration, 0f, MaxValue);
            Satiety = Mathf.Clamp(satiety, 0f, MaxValue);
            Rest = Mathf.Clamp(rest, 0f, MaxValue);
            Health = Mathf.Clamp(health, 0f, MaxValue);
        }
    }
}
